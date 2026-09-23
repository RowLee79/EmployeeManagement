using EmployeeManagement.Application.Positions.Models;

namespace EmployeeManagement.Application.Positions.Interfaces;

public interface IPositionService
{
    Task<IReadOnlyList<PositionListModel>> GetAllAsync();

    Task<IReadOnlyList<PositionListModel>> GetByDepartmentAsync(
        int departmentId);

    Task<IReadOnlyList<PositionLookup>> GetLookupAsync();

    Task<PositionDetailsModel?> GetByIdAsync(int id);

    Task<int> CreateAsync(PositionCreateModel model);

    Task<bool> UpdateAsync(
        int id,
        PositionCreateModel model);

    Task<bool> DeleteAsync(int id);
}