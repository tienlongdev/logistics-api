namespace Logistics.Api.Shipments.Application.Commands.CreateShipment;

public sealed record CreateShipmentResponse(
    Guid ShipmentId,
    string TrackingCode,
    string ShipmentCode,
    string CurrentStatus,
    decimal ShippingFee,
    decimal CodFee,
    decimal TotalFee,
    DateTimeOffset CreatedAt);
