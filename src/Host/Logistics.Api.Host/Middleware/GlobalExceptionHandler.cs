using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Host.Middleware;

/// <summary>
/// Exception handler tập trung, trả về ProblemDetails.
/// Step 2: xử lý generic. Step sau sẽ mở rộng mapping domain/application exceptions.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        _logger.LogError(exception, "Unhandled exception. TraceId={TraceId}", traceId);

        var problem = new ProblemDetails
        {
            Title = "Đã xảy ra lỗi không mong muốn.",
            Detail = httpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
                ? exception.ToString()
                : "Vui lòng liên hệ hỗ trợ và cung cấp CorrelationId để tra cứu log.",
            Status = (int)HttpStatusCode.InternalServerError,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = problem.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
