using EmployeeManagement.Application.Departments.Interfaces;
using EmployeeManagement.Application.Departments.Models;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Application.Common.Interfaces;

namespace EmployeeManagement.Infrastructure.Services;

public class DepartmentService : IDepartmentService
{
    private readonly EmployeeDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DepartmentService(EmployeeDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<DepartmentListModel>> GetAllAsync()
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new DepartmentListModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                EmployeeCount = x.Employees.Count(e => !e.IsDeleted),
                PositionCount = x.Positions.Count(p => !p.IsDeleted)
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<DepartmentLookup>> GetLookupAsync()
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new DepartmentLookup
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync();
    }

    public async Task<DepartmentDetailsModel?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new DepartmentDetailsModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                EmployeeCount = x.Employees.Count(e => !e.IsDeleted),
                PositionCount = x.Positions.Count(p => !p.IsDeleted)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateAsync(DepartmentCreateModel model)
    {
        var exists = await _context.Departments
            .AnyAsync(x =>
                !x.IsDeleted &&
                (x.Code == model.Code || x.Name == model.Name));

        if (exists)
            throw new InvalidOperationException(
                "A department with the same code or name already exists.");

        var department = new Domain.Entities.Department
        {
            Code = model.Code.Trim().ToUpper(),
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            IsActive = model.IsActive,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = _currentUserService.Email ?? "System"
        };

        _context.Departments.Add(department);

        await _context.SaveChangesAsync();

        return department.Id;
    }

    public async Task<bool> UpdateAsync(
        int id,
        DepartmentCreateModel model)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (department is null)
            return false;

        var exists = await _context.Departments
            .AnyAsync(x =>
                x.Id != id &&
                !x.IsDeleted &&
                (x.Code == model.Code || x.Name == model.Name));

        if (exists)
            throw new InvalidOperationException(
                "A department with the same code or name already exists.");

        department.Code = model.Code.Trim().ToUpper();
        department.Name = model.Name.Trim();
        department.Description = model.Description?.Trim();
        department.IsActive = model.IsActive;
        department.UpdatedDate = DateTime.UtcNow;
        department.UpdatedBy = _currentUserService.Email ?? "System";

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (department is null)
            return false;

        var hasEmployees = await _context.Employees
            .AnyAsync(x => x.DepartmentId == id && !x.IsDeleted);

        if (hasEmployees)
            throw new InvalidOperationException(
                "Cannot delete a department that has employees.");

        department.IsDeleted = true;
        department.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}