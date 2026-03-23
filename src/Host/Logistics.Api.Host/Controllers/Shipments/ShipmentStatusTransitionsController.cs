using System.Security.Claims;
using Asp.Versioning;
using Logistics.Api.Host.Extensions;
using Logistics.Api.Shipments.Application.Commands.TransitionShipmentStatus;
using Logistics.Api.Shipments.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Host.Controllers.Shipments;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/shipments/{id:guid}/status-transitions")]
[Authorize(Policy = "HubStaffOrAbove")]
public sealed class ShipmentStatusTransitionsController : ControllerBase
{
    private readonly ISender _sender;

    public ShipmentStatusTransitionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TransitionShipmentStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> TransitionStatus(
        Guid id,
        [FromBody] TransitionShipmentStatusHttpRequest request,
        CancellationToken ct)
    {
        var operatorId = TryGetOperatorId(User);
        var operatorName = User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue(ClaimTypes.Email);

        var command = new TransitionShipmentStatusCommand(
            ShipmentId: id,
            ToStatus: request.ToStatus,
            Note: request.Note,
            HubId: request.HubId,
            HubCode: request.HubCode,
            Location: request.Location,
            OccurredAt: request.OccurredAt,
            OperatorId: operatorId,
            OperatorName: operatorName,
            CorrelationId: TryGetCorrelationId(HttpContext));

        var result = await _sender.Send(command, ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemResult(HttpContext);
    }

    private static Guid? TryGetOperatorId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(raw, out var parsed) ? parsed : null;
    }

    private static Guid? TryGetCorrelationId(HttpContext httpContext)
    {
        var raw = httpContext.Request.Headers["X-Correlation-Id"].ToString();
        if (Guid.TryParse(raw, out var correlationId))
            return correlationId;

        return Guid.TryParse(httpContext.TraceIdentifier, out correlationId)
            ? correlationId
            : null;
    }
}

public sealed record TransitionShipmentStatusHttpRequest(
    ShipmentStatus ToStatus,
    Guid? HubId,
    string? HubCode,
    string? Location,
    string? Note,
    DateTimeOffset? OccurredAt);