using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.Employees.Models;
using EmployeeManagement.Application.Employees.Reports;

namespace EmployeeManagement.Application.Employees.Interfaces;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeListModel>> GetPagedAsync(
        EmployeeSearchModel searchModel);

    Task<EmployeeDetailsModel?> GetByIdAsync(int id);

    Task<int> CreateAsync(EmployeeCreateModel model);

    Task<bool> UpdateAsync(int id, EmployeeEditModel model);

    Task<bool> DeleteAsync(int id);

    Task<IReadOnlyList<EmployeeLookupModel>> GetLookupAsync();

    Task<byte[]> ExportCsvAsync(EmployeeSearchModel searchModel);
    Task<IReadOnlyList<EmployeeReportModel>>
    GetEmployeeReportAsync(EmployeeSearchModel searchModel);

}