using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Shipments.Domain.Enums;

namespace Logistics.Api.Shipments.Application.Commands.CreateShipment;

/// <summary>Nested DTOs for the create shipment command.</summary>
public sealed record AddressDto(
    string Name,
    string Phone,
    string Address,
    string? Province,
    string? District,
    string? Ward);

public sealed record PackageDto(
    int WeightGram,
    int? LengthCm,
    int? WidthCm,
    int? HeightCm,
    string? Description);

/// <summary>
/// Command to create a new shipment.
/// The <see cref="RequestingUserId"/> is extracted from the JWT in the controller
/// and used to enforce merchant scope in the handler.
/// </summary>
public sealed record CreateShipmentCommand(
    Guid RequestingUserId,
    string? IdempotencyKey,
    string? MerchantOrderRef,
    ServiceType ServiceType,
    AddressDto Sender,
    AddressDto Receiver,
    PackageDto Package,
    decimal CodAmount,
    decimal DeclaredValue,
    string? DeliveryNote)
    : ICommand<Result<CreateShipmentResponse>>;
