namespace EmployeeManagement.Application.Reporting
{
    public interface IReportingService
    {
        Task<byte[]> GetEmployeeMasterListPdfAsync(
            EmployeeReportRequest request);
    }
}