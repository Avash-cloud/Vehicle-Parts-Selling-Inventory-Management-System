namespace VehiclePartsSystem.API.Middleware;

/// <summary>
/// Logs all incoming HTTP requests with method, path, status code, and duration.
/// Supports performance monitoring and audit trails.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var start = DateTime.UtcNow;
        await _next(context);
        var duration = (DateTime.UtcNow - start).TotalMilliseconds;

        _logger.LogInformation("{Method} {Path} => {StatusCode} ({Duration}ms)",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            duration);
    }
}
