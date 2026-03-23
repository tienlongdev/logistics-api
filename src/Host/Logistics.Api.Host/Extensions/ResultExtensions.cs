using Logistics.Api.BuildingBlocks.Application.Results;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Host.Extensions;

/// <summary>
/// Maps Application-layer <see cref="Result"/> / <see cref="Result{T}"/> failures
/// to RFC 9457 ProblemDetails HTTP responses for use in MVC controllers.
/// </summary>
public static class ResultExtensions
{
    public static IActionResult ToProblemResult(this Result result, HttpContext httpContext)
    {
        var statusCode = ResolveStatusCode(result.Error.Code);

        var problem = new ProblemDetails
        {
            Title = ResolveTitle(statusCode),
            Detail = result.Error.Message,
            Status = statusCode,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["errorCode"] = result.Error.Code;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static int ResolveStatusCode(string errorCode) => errorCode switch
    {
        var c when c.EndsWith("not_found") => StatusCodes.Status404NotFound,
        var c when c.EndsWith("invalid_state_transition") => StatusCodes.Status409Conflict,
        var c when c.EndsWith("already_exists") => StatusCodes.Status409Conflict,
        var c when c.EndsWith("conflict") => StatusCodes.Status409Conflict,
        var c when c.EndsWith("unauthorized") => StatusCodes.Status401Unauthorized,
        var c when c.EndsWith("forbidden") => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status400BadRequest
    };

    private static string ResolveTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status404NotFound => "Không tìm thấy.",
        StatusCodes.Status409Conflict => "Xung đột dữ liệu.",
        StatusCodes.Status401Unauthorized => "Chưa xác thực.",
        StatusCodes.Status403Forbidden => "Không có quyền truy cập.",
        _ => "Yêu cầu không hợp lệ."
    };
}
