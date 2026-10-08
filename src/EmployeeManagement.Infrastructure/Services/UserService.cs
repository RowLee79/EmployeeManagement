using EmployeeManagement.Application.Users.Interfaces;
using EmployeeManagement.Application.Users.Models;
using EmployeeManagement.Infrastructure.Identity;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly EmployeeDbContext _employeeDbContext;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        EmployeeDbContext employeeDbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _employeeDbContext = employeeDbContext;
    }

    public async Task<IReadOnlyList<UserListModel>> GetAllAsync()
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync();

        var result = new List<UserListModel>();

        foreach (var user in users)
        {
            var roles =
                await _userManager.GetRolesAsync(user);

            string? employeeName = null;

            if (user.EmployeeId.HasValue)
            {
                employeeName =
                    await _employeeDbContext.Employees
                        .Where(x =>
                            x.Id == user.EmployeeId.Value &&
                            !x.IsDeleted)
                        .Select(x =>
                            x.FirstName + " " + x.LastName)
                        .FirstOrDefaultAsync();
            }

            result.Add(new UserListModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = !user.LockoutEnabled ||
                           user.LockoutEnd == null ||
                           user.LockoutEnd <= DateTimeOffset.UtcNow,
                EmployeeId = user.EmployeeId,
                EmployeeName = employeeName,
                Roles = roles.ToList()
            });
        }

        return result;
    }


    public async Task<UserDetailsModel?> GetByIdAsync(
        string id)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user is null)
            return null;

        var roles =
            await _userManager.GetRolesAsync(user);

        string? employeeName = null;

        if (user.EmployeeId.HasValue)
        {
            employeeName =
                await _employeeDbContext.Employees
                    .Where(x =>
                        x.Id == user.EmployeeId.Value &&
                        !x.IsDeleted)
                    .Select(x =>
                        x.FirstName + " " + x.LastName)
                    .FirstOrDefaultAsync();
        }

        return new UserDetailsModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            IsActive =
                !user.LockoutEnabled ||
                user.LockoutEnd == null ||
                user.LockoutEnd <= DateTimeOffset.UtcNow,
            EmployeeId = user.EmployeeId,
            EmployeeName = employeeName,
            Roles = roles.ToList(),
            LockoutEnd = user.LockoutEnd,
            IsLockedOut =
                user.LockoutEnd.HasValue &&
                user.LockoutEnd > DateTimeOffset.UtcNow
        };
    }


    public async Task<string> CreateAsync(
        UserCreateModel model)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(model.Email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "A user with this email address already exists.");
        }

        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            throw new InvalidOperationException(
                "The selected role does not exist.");
        }

        if (model.EmployeeId.HasValue)
        {
            var employeeExists =
                await _employeeDbContext.Employees.AnyAsync(
                    x =>
                        x.Id == model.EmployeeId.Value &&
                        !x.IsDeleted);

            if (!employeeExists)
            {
                throw new InvalidOperationException(
                    "The selected employee does not exist.");
            }
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            EmployeeId = model.EmployeeId,
            EmailConfirmed = true
        };

        var createResult =
            await _userManager.CreateAsync(
                user,
                model.Password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    " ",
                    createResult.Errors.Select(x => x.Description)));
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                model.Role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            throw new InvalidOperationException(
                string.Join(
                    " ",
                    roleResult.Errors.Select(x => x.Description)));
        }

        if (!model.IsActive)
        {
            await SetActiveAsync(
                user.Id,
                false);
        }

        return user.Id;
    }


    public async Task<bool> UpdateAsync(
        UserEditModel model)
    {
        var user =
            await _userManager.FindByIdAsync(model.Id);

        if (user is null)
            return false;

        var emailOwner =
            await _userManager.FindByEmailAsync(model.Email);

        if (emailOwner is not null &&
            emailOwner.Id != model.Id)
        {
            throw new InvalidOperationException(
                "Another user already uses this email address.");
        }

        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            throw new InvalidOperationException(
                "The selected role does not exist.");
        }

        if (model.EmployeeId.HasValue)
        {
            var employeeExists =
                await _employeeDbContext.Employees.AnyAsync(
                    x =>
                        x.Id == model.EmployeeId.Value &&
                        !x.IsDeleted);

            if (!employeeExists)
            {
                throw new InvalidOperationException(
                    "The selected employee does not exist.");
            }
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.EmployeeId = model.EmployeeId;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    " ",
                    updateResult.Errors.Select(x => x.Description)));
        }

        var currentRoles =
            await _userManager.GetRolesAsync(user);

        if (!currentRoles.Contains(model.Role))
        {
            if (currentRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);
            }

            await _userManager.AddToRoleAsync(
                user,
                model.Role);
        }

        await SetActiveAsync(
            user.Id,
            model.IsActive);

        return true;
    }


    public async Task<bool> SetActiveAsync(
        string id,
        bool isActive)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user is null)
            return false;

        if (isActive)
        {
            user.LockoutEnabled = false;
            user.LockoutEnd = null;
        }
        else
        {
            user.LockoutEnabled = true;
            user.LockoutEnd =
                DateTimeOffset.UtcNow.AddYears(100);
        }

        var result =
            await _userManager.UpdateAsync(user);

        return result.Succeeded;
    }


    public async Task<bool> ResetPasswordAsync(
        string id,
        string newPassword)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user is null)
            return false;

        var token =
            await _userManager.GeneratePasswordResetTokenAsync(
                user);

        var result =
            await _userManager.ResetPasswordAsync(
                user,
                token,
                newPassword);

        return result.Succeeded;
    }


    public async Task<bool> DeleteAsync(
        string id)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user is null)
            return false;

        var result =
            await _userManager.DeleteAsync(user);

        return result.Succeeded;
    }


    public async Task<IReadOnlyList<string>> GetRolesAsync()
    {
        return await _roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => x.Name!)
            .ToListAsync();
    }


    public async Task<IReadOnlyList<EmployeeLookupModel>>
        GetEmployeeLookupAsync()
    {
        return await _employeeDbContext.Employees
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.Status ==
                    EmployeeManagement.Domain.Enums.EmployeeStatus.Active)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new EmployeeLookupModel
            {
                Id = x.Id,
                EmployeeNumber = x.EmployeeNumber,
                FullName =
                    x.FirstName + " " + x.LastName
            })
            .ToListAsync();
    }
}