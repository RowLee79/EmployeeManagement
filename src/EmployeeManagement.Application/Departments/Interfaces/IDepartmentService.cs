using EmployeeManagement.Application.Departments.Models;

namespace EmployeeManagement.Application.Departments.Interfaces;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentListModel>> GetAllAsync();

    Task<IReadOnlyList<DepartmentLookup>> GetLookupAsync();

    Task<DepartmentDetailsModel?> GetByIdAsync(int id);

    Task<int> CreateAsync(DepartmentCreateModel model);

    Task<bool> UpdateAsync(int id, DepartmentCreateModel model);

    Task<bool> DeleteAsync(int id);
}

