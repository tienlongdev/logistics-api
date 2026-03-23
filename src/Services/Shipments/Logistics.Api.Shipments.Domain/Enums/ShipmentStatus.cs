namespace Logistics.Api.Shipments.Domain.Enums;

/// <summary>
/// Lifecycle statuses for a Shipment.
/// Terminal states: Delivered, Returned, Cancelled.
/// </summary>
public enum ShipmentStatus
{
    Created = 1,
    AwaitingPickup = 2,
    PickedUp = 3,
    InTransit = 4,
    OutForDelivery = 5,
    Delivered = 6,
    DeliveryFailed = 7,
    Returning = 8,
    Returned = 9,
    Cancelled = 10,
}
