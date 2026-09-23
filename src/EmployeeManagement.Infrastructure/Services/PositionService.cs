using EmployeeManagement.Application.Common.Interfaces;
using EmployeeManagement.Application.Positions.Models;
using EmployeeManagement.Application.Positions.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Services;

public class PositionService :IPositionService
{
    private readonly EmployeeDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    public PositionService(EmployeeDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<PositionListModel>> GetAllAsync()
    {
        return await _context.Positions
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new PositionListModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                MinimumSalary = x.MinimumSalary,
                MaximumSalary = x.MaximumSalary,
                IsActive = x.IsActive,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department.Name,
                EmployeeCount = x.Employees.Count(e => !e.IsDeleted)
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PositionListModel>> GetByDepartmentAsync(
        int departmentId)
    {
        return await _context.Positions
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.IsActive &&
                x.DepartmentId == departmentId)
            .OrderBy(x => x.Name)
            .Select(x => new PositionListModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department.Name,
                IsActive = x.IsActive,
                MinimumSalary = x.MinimumSalary,
                MaximumSalary = x.MaximumSalary,
                EmployeeCount = x.Employees.Count(e => !e.IsDeleted)
            })
            .ToListAsync();
    }

    public async Task<PositionDetailsModel?> GetByIdAsync(int id)
    {
        return await _context.Positions
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new PositionDetailsModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                MinimumSalary = x.MinimumSalary,
                MaximumSalary = x.MaximumSalary,
                IsActive = x.IsActive,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department.Name,
                EmployeeCount = x.Employees.Count(e => !e.IsDeleted)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateAsync(PositionCreateModel model)
    {
        if (model.MinimumSalary.HasValue &&
            model.MaximumSalary.HasValue &&
            model.MinimumSalary > model.MaximumSalary)
        {
            throw new InvalidOperationException(
                "Minimum salary cannot be greater than maximum salary.");
        }

        var exists = await _context.Positions
            .AnyAsync(x =>
                !x.IsDeleted &&
                (x.Code == model.Code ||
                 (x.Name == model.Name &&
                  x.DepartmentId == model.DepartmentId)));

        if (exists)
            throw new InvalidOperationException(
                "A position with the same code or name already exists.");

        var position = new Domain.Entities.Position
        {
            Code = model.Code.Trim().ToUpper(),
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            MinimumSalary = model.MinimumSalary,
            MaximumSalary = model.MaximumSalary,
            IsActive = model.IsActive,
            DepartmentId = model.DepartmentId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = _currentUserService.Email ?? "System"

        };

        _context.Positions.Add(position);

        await _context.SaveChangesAsync();

        return position.Id;
    }

    public async Task<bool> UpdateAsync(
        int id,
        PositionCreateModel model)
    {
        var position = await _context.Positions
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (position is null)
            return false;

        if (model.MinimumSalary.HasValue &&
            model.MaximumSalary.HasValue &&
            model.MinimumSalary > model.MaximumSalary)
        {
            throw new InvalidOperationException(
                "Minimum salary cannot be greater than maximum salary.");
        }

        var exists = await _context.Positions
            .AnyAsync(x =>
                x.Id != id &&
                !x.IsDeleted &&
                (x.Code == model.Code ||
                 (x.Name == model.Name &&
                  x.DepartmentId == model.DepartmentId)));

        if (exists)
            throw new InvalidOperationException(
                "A position with the same code or name already exists.");

        position.Code = model.Code.Trim().ToUpper();
        position.Name = model.Name.Trim();
        position.Description = model.Description?.Trim();
        position.MinimumSalary = model.MinimumSalary;
        position.MaximumSalary = model.MaximumSalary;
        position.IsActive = model.IsActive;
        position.DepartmentId = model.DepartmentId;
        position.UpdatedDate = DateTime.UtcNow;
        position.UpdatedBy = _currentUserService.Email ?? "System";

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var position = await _context.Positions
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (position is null)
            return false;

        var hasEmployees = await _context.Employees
            .AnyAsync(x =>
                x.PositionId == id &&
                !x.IsDeleted);

        if (hasEmployees)
            throw new InvalidOperationException(
                "Cannot delete a position that has employees.");

        position.IsDeleted = true;
        position.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IReadOnlyList<PositionLookup>> GetLookupAsync()
    {
        var databaseName =
            _context.Database.GetDbConnection().Database;

        var count =
            await _context.Positions.CountAsync();

        Console.WriteLine($"POSITION COUNT FROM EF: {count}");

        var positions =
            await _context.Positions
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new PositionLookup
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    DepartmentId = x.DepartmentId
                })
                .ToListAsync();


        return positions;
    }
}