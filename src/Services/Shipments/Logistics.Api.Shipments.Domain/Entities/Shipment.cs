using Logistics.Api.BuildingBlocks.Domain.Abstractions;
using Logistics.Api.BuildingBlocks.Domain.Time;
using Logistics.Api.Shipments.Domain.Enums;
using Logistics.Api.Shipments.Domain.Services;

namespace Logistics.Api.Shipments.Domain.Entities;

/// <summary>
/// Shipment aggregate root.
///
/// Invariants:
///   - Status transitions must follow <see cref="ShipmentStateMachine"/>.
///   - Every status change appends a <see cref="TrackingEvent"/>.
///   - Fees are immutable after creation (repriced only via a dedicated command).
/// </summary>
public sealed class Shipment : AggregateRoot<Guid>
{
    private readonly List<TrackingEvent> _trackingEvents = new();

    // Required by EF Core
    private Shipment() { }

    // ── Identity ──────────────────────────────────────────────────────────────
    public string TrackingCode { get; private set; } = default!;
    public string ShipmentCode { get; private set; } = default!;
    public string? IdempotencyKey { get; private set; }

    // ── Merchant ─────────────────────────────────────────────────────────────
    public Guid MerchantId { get; private set; }
    public string MerchantCode { get; private set; } = default!;

    // ── Sender ───────────────────────────────────────────────────────────────
    public string SenderName { get; private set; } = default!;
    public string SenderPhone { get; private set; } = default!;
    public string SenderAddress { get; private set; } = default!;
    public string? SenderProvince { get; private set; }
    public string? SenderDistrict { get; private set; }
    public string? SenderWard { get; private set; }

    // ── Receiver ─────────────────────────────────────────────────────────────
    public string ReceiverName { get; private set; } = default!;
    public string ReceiverPhone { get; private set; } = default!;
    public string ReceiverAddress { get; private set; } = default!;
    public string? ReceiverProvince { get; private set; }
    public string? ReceiverDistrict { get; private set; }
    public string? ReceiverWard { get; private set; }

    // ── Package ───────────────────────────────────────────────────────────────
    public int WeightGram { get; private set; }
    public int? LengthCm { get; private set; }
    public int? WidthCm { get; private set; }
    public int? HeightCm { get; private set; }
    public string? PackageDescription { get; private set; }
    public decimal DeclaredValue { get; private set; }

    // ── Financials ────────────────────────────────────────────────────────────
    public decimal CodAmount { get; private set; }
    public decimal ShippingFee { get; private set; }
    public decimal InsuranceFee { get; private set; }
    public decimal TotalFee { get; private set; }

    // ── Service ───────────────────────────────────────────────────────────────
    public ServiceType ServiceType { get; private set; }
    public string? DeliveryNote { get; private set; }

    // ── Status & hub ─────────────────────────────────────────────────────────
    public ShipmentStatus CurrentStatus { get; private set; }
    public Guid? CurrentHubId { get; private set; }
    public string? CurrentHubCode { get; private set; }

    // ── Lifecycle ────────────────────────────────────────────────────────────
    public DateOnly? ExpectedDelivery { get; private set; }
    public DateTimeOffset? ActualDeliveredAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancelledReason { get; private set; }

    // ── Metadata ─────────────────────────────────────────────────────────────
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<TrackingEvent> TrackingEvents => _trackingEvents.AsReadOnly();

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Shipment Create(
        string trackingCode,
        string shipmentCode,
        string? idempotencyKey,
        Guid merchantId,
        string merchantCode,
        string senderName,
        string senderPhone,
        string senderAddress,
        string? senderProvince,
        string? senderDistrict,
        string? senderWard,
        string receiverName,
        string receiverPhone,
        string receiverAddress,
        string? receiverProvince,
        string? receiverDistrict,
        string? receiverWard,
        int weightGram,
        int? lengthCm,
        int? widthCm,
        int? heightCm,
        string? packageDescription,
        decimal declaredValue,
        decimal codAmount,
        decimal shippingFee,
        decimal insuranceFee,
        decimal totalFee,
        ServiceType serviceType,
        string? deliveryNote,
        DateTimeOffset createdAt)
    {
        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            TrackingCode = trackingCode,
            ShipmentCode = shipmentCode,
            IdempotencyKey = idempotencyKey,
            MerchantId = merchantId,
            MerchantCode = merchantCode,
            SenderName = senderName,
            SenderPhone = senderPhone,
            SenderAddress = senderAddress,
            SenderProvince = senderProvince,
            SenderDistrict = senderDistrict,
            SenderWard = senderWard,
            ReceiverName = receiverName,
            ReceiverPhone = receiverPhone,
            ReceiverAddress = receiverAddress,
            ReceiverProvince = receiverProvince,
            ReceiverDistrict = receiverDistrict,
            ReceiverWard = receiverWard,
            WeightGram = weightGram,
            LengthCm = lengthCm,
            WidthCm = widthCm,
            HeightCm = heightCm,
            PackageDescription = packageDescription,
            DeclaredValue = declaredValue,
            CodAmount = codAmount,
            ShippingFee = shippingFee,
            InsuranceFee = insuranceFee,
            TotalFee = totalFee,
            ServiceType = serviceType,
            DeliveryNote = deliveryNote,
            CurrentStatus = ShipmentStatus.Created,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };

        // Append initial tracking event
        shipment._trackingEvents.Add(TrackingEvent.Create(
            shipmentId: shipment.Id,
            trackingCode: trackingCode,
            fromStatus: null,
            toStatus: ShipmentStatus.Created,
            note: "Đơn hàng được tạo.",
            source: "API",
            occurredAt: createdAt));

        return shipment;
    }

    // ── Domain behaviour ─────────────────────────────────────────────────────

    /// <summary>
    /// Attempts a status transition.
    /// Returns <c>false</c> if the transition is not allowed by the state machine,
    /// in which case the caller should map to a 409 Conflict response.
    /// </summary>
    public bool TryTransition(
        ShipmentStatus toStatus,
        string? note,
        Guid? operatorId,
        string? operatorName,
        Guid? hubId,
        string? hubCode,
        string? location,
        string source,
        IClock clock)
    {
        if (!ShipmentStateMachine.CanTransition(CurrentStatus, toStatus))
            return false;

        var fromStatus = CurrentStatus;
        CurrentStatus = toStatus;
        UpdatedAt = clock.UtcNow;

        if (toStatus == ShipmentStatus.Cancelled)
        {
            CancelledAt = clock.UtcNow;
            CancelledReason = note;
        }

        if (toStatus == ShipmentStatus.Delivered)
            ActualDeliveredAt = clock.UtcNow;

        if (hubId.HasValue)
        {
            CurrentHubId = hubId;
            CurrentHubCode = hubCode;
        }

        _trackingEvents.Add(TrackingEvent.Create(
            shipmentId: Id,
            trackingCode: TrackingCode,
            fromStatus: fromStatus,
            toStatus: toStatus,
            hubId: hubId,
            hubCode: hubCode,
            location: location,
            note: note,
            operatorId: operatorId,
            operatorName: operatorName,
            source: source,
            occurredAt: clock.UtcNow));

        return true;
    }
}
