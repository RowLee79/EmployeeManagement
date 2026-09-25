using Microsoft.AspNetCore.Diagnostics;

namespace EmployeeManagement.Web.Logging;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId =
            httpContext.Items["X-Correlation-ID"]?.ToString()
            ?? Guid.NewGuid().ToString("N");

        _logger.LogError(
            exception,
            "Unhandled exception. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
            correlationId,
            httpContext.Request.Path,
            httpContext.Request.Method);

        httpContext.Response.Redirect(
            $"/Error?correlationId={correlationId}");

        await Task.CompletedTask;

        return true;
    }
}