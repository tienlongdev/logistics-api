using Asp.Versioning;
using Logistics.Api.Host.Extensions;
using Logistics.Api.Identity.Application.Commands.Login;
using Logistics.Api.Identity.Application.Commands.Logout;
using Logistics.Api.Identity.Application.Commands.Refresh;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Host.Controllers.Auth;

/// <summary>
/// Authentication endpoints: login, refresh, logout.
/// Controllers are thin: they only translate HTTP → Command and Result → HTTP.
/// All business logic lives in the Application layer.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    /// <summary>Authenticate and receive JWT + refresh token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString());

        var result = await _sender.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemResult(HttpContext);
    }

    /// <summary>Rotate a refresh token and get a new access + refresh token pair.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var command = new RefreshTokenCommand(
            request.RefreshToken,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString());

        var result = await _sender.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemResult(HttpContext);
    }

    /// <summary>Revoke a refresh token (idempotent).</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        var command = new LogoutCommand(request.RefreshToken);
        await _sender.Send(command, ct);
        return NoContent();
    }
}

// ── Request / Response DTOs (controller-layer only) ──────────────────────────

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn);
