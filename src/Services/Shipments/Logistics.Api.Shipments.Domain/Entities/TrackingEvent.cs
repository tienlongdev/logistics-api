using Logistics.Api.BuildingBlocks.Domain.Abstractions;
using Logistics.Api.Shipments.Domain.Enums;

namespace Logistics.Api.Shipments.Domain.Entities;

/// <summary>
/// Records a single status transition for a shipment (the audit trail).
/// Append-only — never mutated after creation.
/// </summary>
public sealed class TrackingEvent : Entity<Guid>
{
    // Required by EF Core
    private TrackingEvent() { }

    public Guid ShipmentId { get; private set; }
    public string TrackingCode { get; private set; } = default!;

    /// <summary>The status before this transition (null for the initial "Created" event).</summary>
    public ShipmentStatus? FromStatus { get; private set; }

    public ShipmentStatus ToStatus { get; private set; }

    public Guid? HubId { get; private set; }
    public string? HubCode { get; private set; }
    public string? Location { get; private set; }
    public string? Note { get; private set; }

    public Guid? OperatorId { get; private set; }
    public string? OperatorName { get; private set; }

    /// <summary>Source system: API | WORKER | SYSTEM | SCAN</summary>
    public string Source { get; private set; } = "SYSTEM";

    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static TrackingEvent Create(
        Guid shipmentId,
        string trackingCode,
        ShipmentStatus? fromStatus,
        ShipmentStatus toStatus,
        Guid? hubId = null,
        string? hubCode = null,
        string? location = null,
        string? note = null,
        Guid? operatorId = null,
        string? operatorName = null,
        string source = "SYSTEM",
        DateTimeOffset occurredAt = default)
    {
        var now = DateTimeOffset.UtcNow;
        return new TrackingEvent
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            TrackingCode = trackingCode,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            HubId = hubId,
            HubCode = hubCode,
            Location = location,
            Note = note,
            OperatorId = operatorId,
            OperatorName = operatorName,
            Source = source,
            OccurredAt = occurredAt == default ? now : occurredAt,
            CreatedAt = now,
        };
    }
}
