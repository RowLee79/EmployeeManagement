using EmployeeManagement.Application.Reporting;

namespace EmployeeManagement.Web.Services.Reporting;

public interface IReportingService
{
    Task<byte[]> GenerateEmployeeMasterListAsync(
        EmployeeReportRequest request,
        string format);
}