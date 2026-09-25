using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.Employees.Models;

namespace EmployeeManagement.Application.Employees.Interfaces;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeListModel>> GetPagedAsync(
        EmployeeSearchModel searchModel);

    Task<IReadOnlyList<EmployeeLookupModel>> GetLookupAsync();

    Task<EmployeeDetailsModel?> GetByIdAsync(int id);

    Task<int> CreateAsync(EmployeeCreateModel model);

    Task<bool> UpdateAsync(
        int id,
        EmployeeEditModel model);

    Task<bool> DeleteAsync(int id);

    Task<IReadOnlyList<EmployeeListModel>> GetDeletedAsync();

    Task<bool> RestoreAsync(int id);

    Task<byte[]> ExportCsvAsync(
        EmployeeSearchModel searchModel);

    Task<IReadOnlyList<EmployeeReportModel>>
        GetEmployeeReportAsync(
            EmployeeSearchModel searchModel);

    Task<EmployeeDetailsModel?> GetDeletedByIdAsync(int id);

    Task<bool> PermanentlyDeleteAsync(int id);
}