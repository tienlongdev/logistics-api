using System.Security.Claims;
using Asp.Versioning;
using Logistics.Api.Host.Extensions;
using Logistics.Api.Shipments.Application.Commands.CreateShipment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Host.Controllers.Shipments;

/// <summary>
/// Shipments endpoints.
/// All write operations require an authenticated Merchant user.
/// Controllers are thin: they only translate HTTP → Command and Result → HTTP.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/shipments")]
[Authorize(Policy = "MerchantOrAdmin")]
public sealed class ShipmentsController : ControllerBase
{
    private readonly ISender _sender;

    public ShipmentsController(ISender sender) => _sender = sender;

    /// <summary>Create a new shipment. Idempotency-Key header is required.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateShipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateShipment(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromBody] CreateShipmentHttpRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !Guid.TryParse(userId, out var requestingUserId))
            return Unauthorized();

        var command = new CreateShipmentCommand(
            RequestingUserId: requestingUserId,
            IdempotencyKey: idempotencyKey,
            MerchantOrderRef: request.MerchantOrderRef,
            ServiceType: request.ServiceType,
            Sender: new AddressDto(
                                  request.Sender.Name, request.Sender.Phone, request.Sender.Address,
                                  request.Sender.Province, request.Sender.District, request.Sender.Ward),
            Receiver: new AddressDto(
                                  request.Receiver.Name, request.Receiver.Phone, request.Receiver.Address,
                                  request.Receiver.Province, request.Receiver.District, request.Receiver.Ward),
            Package: new PackageDto(
                                  request.Package.WeightGram, request.Package.LengthCm,
                                  request.Package.WidthCm, request.Package.HeightCm,
                                  request.Package.Description),
            CodAmount: request.CodAmount,
            DeclaredValue: request.DeclaredValue,
            DeliveryNote: request.DeliveryNote);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return result.ToProblemResult(HttpContext);

        return CreatedAtAction(
            actionName: nameof(CreateShipment),
            routeValues: new { id = result.Value.ShipmentId },
            value: result.Value);
    }
}

// ── HTTP request model ─────────────────────────────────────────────────────────

/// <summary>HTTP request body for POST /api/v1/shipments.</summary>
public sealed record CreateShipmentHttpRequest(
    string? MerchantOrderRef,
    Logistics.Api.Shipments.Domain.Enums.ServiceType ServiceType,
    AddressHttpDto Sender,
    AddressHttpDto Receiver,
    PackageHttpDto Package,
    decimal CodAmount,
    decimal DeclaredValue,
    string? DeliveryNote);

public sealed record AddressHttpDto(
    string Name,
    string Phone,
    string Address,
    string? Province,
    string? District,
    string? Ward);

public sealed record PackageHttpDto(
    int WeightGram,
    int? LengthCm,
    int? WidthCm,
    int? HeightCm,
    string? Description);
