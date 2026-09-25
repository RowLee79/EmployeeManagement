using System.Text;
using System.Text.Json;
using EmployeeManagement.Application.Reporting;

namespace EmployeeManagement.Web.Services.Reporting;

public class ReportingService : IReportingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReportingService> _logger;

    public ReportingService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ReportingService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<byte[]> GenerateEmployeeMasterListAsync(
        EmployeeReportRequest request,
        string format)
    {
        var baseUrl =
            _configuration["Reporting:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "Reporting:BaseUrl is not configured.");
        }

        var url =
            $"{baseUrl.TrimEnd('/')}/Reports/EmployeeMasterList";

        var payload = new
        {
            request.Search,
            request.DepartmentId,
            request.PositionId,
            request.EmploymentType,
            request.Status,
            request.HireDateFrom,
            request.HireDateTo,

            // IMPORTANT
            Format = format
        };

        var json =
            JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy =
                        JsonNamingPolicy.CamelCase
                });

        _logger.LogInformation(
            "Sending report request. Format: {Format}",
            format);

        using var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        using var response =
            await _httpClient.PostAsync(
                url,
                content);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content
                    .ReadAsStringAsync();

            _logger.LogError(
                "Reporting service failed. " +
                "Status: {StatusCode}, Response: {Response}",
                response.StatusCode,
                error);

            throw new HttpRequestException(
                $"Reporting service returned " +
                $"{(int)response.StatusCode}.");
        }

        return await response.Content
            .ReadAsByteArrayAsync();
    }
}