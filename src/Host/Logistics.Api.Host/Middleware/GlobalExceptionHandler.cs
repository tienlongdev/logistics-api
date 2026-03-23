using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Host.Middleware;

/// <summary>
/// Centralised exception handler.
/// Maps FluentValidation.ValidationException → 400 ValidationProblemDetails (A2).
/// Maps all other exceptions → 500 ProblemDetails.
/// Always includes traceId (= CorrelationId set by CorrelationIdMiddleware).
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var validationProblem = new ValidationProblemDetails(errors)
            {
                Title = "Dữ liệu đầu vào không hợp lệ.",
                Status = StatusCodes.Status400BadRequest,
                Instance = httpContext.Request.Path
            };
            validationProblem.Extensions["traceId"] = traceId;

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
            return true;
        }

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

        httpContext.Response.StatusCode = problem.Status!.Value;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}

