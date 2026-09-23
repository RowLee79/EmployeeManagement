using System.Net.Http.Json;

namespace EmployeeManagement.Application.Reporting;

public class ReportingService : IReportingService
{
    private readonly HttpClient _httpClient;

    public ReportingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> GetEmployeeMasterListPdfAsync(
        EmployeeReportRequest request)
    {
        using var response =
            await _httpClient.PostAsJsonAsync(
                "/Reports/EmployeeMasterList",
                request);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadAsByteArrayAsync();
    }
}