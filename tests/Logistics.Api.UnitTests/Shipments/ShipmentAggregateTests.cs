using FluentAssertions;
using Logistics.Api.BuildingBlocks.Domain.Time;
using Logistics.Api.Shipments.Domain.Entities;
using Logistics.Api.Shipments.Domain.Enums;
using NSubstitute;

namespace Logistics.Api.UnitTests.Shipments;

public sealed class ShipmentAggregateTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 3, 23, 10, 0, 0, TimeSpan.Zero);

    // ── helpers ───────────────────────────────────────────────────────────────

    private static IClock FakeClock(DateTimeOffset? at = null)
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(at ?? FixedNow);
        return clock;
    }

    private static Shipment CreateTestShipment(string? idempotencyKey = "idem-001") =>
        Shipment.Create(
            trackingCode: "LGA26030000001",
            shipmentCode: "SHIP-20260323-0001",
            idempotencyKey: idempotencyKey,
            merchantId: Guid.NewGuid(),
            merchantCode: "MERCH-001",
            senderName: "Nguyen Van A",
            senderPhone: "0901234567",
            senderAddress: "123 Le Loi",
            senderProvince: "Ho Chi Minh",
            senderDistrict: "Quan 1",
            senderWard: "Ben Nghe",
            receiverName: "Tran Thi B",
            receiverPhone: "0987654321",
            receiverAddress: "456 Tran Hung Dao",
            receiverProvince: "Ha Noi",
            receiverDistrict: "Hoan Kiem",
            receiverWard: "Hang Bai",
            weightGram: 1000,
            lengthCm: 20,
            widthCm: 15,
            heightCm: 10,
            packageDescription: "Clothes",
            declaredValue: 500_000m,
            codAmount: 300_000m,
            shippingFee: 30_000m,
            insuranceFee: 0m,
            totalFee: 30_000m,
            serviceType: ServiceType.Standard,
            deliveryNote: null,
            createdAt: FixedNow);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_SetsInitialStatusToCreated()
    {
        var shipment = CreateTestShipment();

        shipment.CurrentStatus.Should().Be(ShipmentStatus.Created);
    }

    [Fact]
    public void Create_AppendsExactlyOneTrackingEvent()
    {
        var shipment = CreateTestShipment();

        shipment.TrackingEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Create_InitialTrackingEvent_HasCreatedStatus()
    {
        var shipment = CreateTestShipment();

        var evt = shipment.TrackingEvents[0];
        evt.ToStatus.Should().Be(ShipmentStatus.Created);
        evt.FromStatus.Should().BeNull();
    }

    [Fact]
    public void Create_InitialTrackingEvent_HasCorrectTrackingCode()
    {
        var shipment = CreateTestShipment();

        shipment.TrackingEvents[0].TrackingCode.Should().Be("LGA26030000001");
    }

    [Fact]
    public void Create_SetsCreatedAt()
    {
        var shipment = CreateTestShipment();

        shipment.CreatedAt.Should().Be(FixedNow);
    }

    [Fact]
    public void Create_WithNullIdempotencyKey_DoesNotThrow()
    {
        var action = () => CreateTestShipment(idempotencyKey: null);

        action.Should().NotThrow();
    }

    // ── TryTransition — success ───────────────────────────────────────────────

    [Fact]
    public void TryTransition_ReturnsTrue_ForValidTransition()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        var result = shipment.TryTransition(ShipmentStatus.AwaitingPickup,
            note: null, operatorId: null, operatorName: null,
            hubId: null, hubCode: null, location: null, source: "test", clock);

        result.Should().BeTrue();
    }

    [Fact]
    public void TryTransition_UpdatesCurrentStatus()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        shipment.TryTransition(ShipmentStatus.AwaitingPickup,
            null, null, null, null, null, null, "test", clock);

        shipment.CurrentStatus.Should().Be(ShipmentStatus.AwaitingPickup);
    }

    [Fact]
    public void TryTransition_AppendsNewTrackingEvent()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        shipment.TryTransition(ShipmentStatus.AwaitingPickup,
            null, null, null, null, null, null, "test", clock);

        shipment.TrackingEvents.Should().HaveCount(2);
    }

    [Fact]
    public void TryTransition_NewTrackingEvent_HasCorrectFromAndToStatus()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        shipment.TryTransition(ShipmentStatus.AwaitingPickup,
            null, null, null, null, null, null, "test", clock);

        var latest = shipment.TrackingEvents[^1];
        latest.FromStatus.Should().Be(ShipmentStatus.Created);
        latest.ToStatus.Should().Be(ShipmentStatus.AwaitingPickup);
    }

    [Fact]
    public void TryTransition_UpdatesHub_WhenHubIdProvided()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();
        var hubId = Guid.NewGuid();

        shipment.TryTransition(ShipmentStatus.AwaitingPickup,
            null, null, null, hubId, "HUB-01", null, "system", clock);

        shipment.CurrentHubId.Should().Be(hubId);
        shipment.CurrentHubCode.Should().Be("HUB-01");
    }

    [Fact]
    public void TryTransition_DoesNotUpdateHub_WhenHubIdNotProvided()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        shipment.TryTransition(ShipmentStatus.AwaitingPickup,
            null, null, null, null, null, null, "system", clock);

        shipment.CurrentHubId.Should().BeNull();
    }

    // ── TryTransition — Cancelled side-effects ────────────────────────────────

    [Fact]
    public void TryTransition_ToCancelled_SetsCancelledAt()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        shipment.TryTransition(ShipmentStatus.Cancelled,
            "Customer request", null, null, null, null, null, "test", clock);

        shipment.CancelledAt.Should().Be(FixedNow);
    }

    [Fact]
    public void TryTransition_ToCancelled_SetsCancelledReason()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        shipment.TryTransition(ShipmentStatus.Cancelled,
            "Customer request", null, null, null, null, null, "test", clock);

        shipment.CancelledReason.Should().Be("Customer request");
    }

    // ── TryTransition — Delivered side-effects ────────────────────────────────

    [Fact]
    public void TryTransition_ToDelivered_SetsActualDeliveredAt()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        // Drive through the happy path to reach Delivered
        shipment.TryTransition(ShipmentStatus.AwaitingPickup, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.PickedUp, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.InTransit, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.OutForDelivery, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.Delivered, null, null, null, null, null, null, "s", clock);

        shipment.ActualDeliveredAt.Should().Be(FixedNow);
    }

    // ── TryTransition — failure ───────────────────────────────────────────────

    [Fact]
    public void TryTransition_ReturnsFalse_ForInvalidTransition()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        var result = shipment.TryTransition(ShipmentStatus.Delivered,
            null, null, null, null, null, null, "test", clock);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryTransition_DoesNotChangeStatus_WhenTransitionInvalid()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        shipment.TryTransition(ShipmentStatus.Delivered,
            null, null, null, null, null, null, "test", clock);

        shipment.CurrentStatus.Should().Be(ShipmentStatus.Created);
    }

    [Fact]
    public void TryTransition_DoesNotAppendTrackingEvent_WhenTransitionInvalid()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        shipment.TryTransition(ShipmentStatus.Delivered,
            null, null, null, null, null, null, "test", clock);

        shipment.TrackingEvents.Should().HaveCount(1, "no event appended for invalid transition");
    }

    [Fact]
    public void TryTransition_ReturnsFalse_FromTerminalState()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        shipment.TryTransition(ShipmentStatus.Cancelled, null, null, null, null, null, null, "test", clock);

        // Try any further transition from terminal
        var result = shipment.TryTransition(ShipmentStatus.AwaitingPickup,
            null, null, null, null, null, null, "test", clock);

        result.Should().BeFalse();
    }

    // ── Multi-step transition — happy path ────────────────────────────────────

    [Fact]
    public void TryTransition_FullHappyPath_ProducesCorrectEventCount()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        // Created(1) → AwaitingPickup → PickedUp → InTransit → OutForDelivery → Delivered
        shipment.TryTransition(ShipmentStatus.AwaitingPickup, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.PickedUp, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.InTransit, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.OutForDelivery, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.Delivered, null, null, null, null, null, null, "s", clock);

        // 1 initial Create event + 5 transitions
        shipment.TrackingEvents.Should().HaveCount(6);
        shipment.CurrentStatus.Should().Be(ShipmentStatus.Delivered);
    }

    [Fact]
    public void TryTransition_ReturnPath_ProducesCorrectFinalStatus()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        shipment.TryTransition(ShipmentStatus.AwaitingPickup, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.PickedUp, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.InTransit, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.Returning, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.Returned, null, null, null, null, null, null, "s", clock);

        shipment.CurrentStatus.Should().Be(ShipmentStatus.Returned);
    }

    [Fact]
    public void TryTransition_DeliveryFailed_CanRetryDelivery()
    {
        var shipment = CreateTestShipment();
        var clock = FakeClock();

        shipment.TryTransition(ShipmentStatus.AwaitingPickup, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.PickedUp, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.InTransit, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.OutForDelivery, null, null, null, null, null, null, "s", clock);
        shipment.TryTransition(ShipmentStatus.DeliveryFailed, "Recipient absent", null, null, null, null, null, "s", clock);

        var retryResult = shipment.TryTransition(ShipmentStatus.OutForDelivery,
            null, null, null, null, null, null, "s", clock);

        retryResult.Should().BeTrue();
        shipment.CurrentStatus.Should().Be(ShipmentStatus.OutForDelivery);
    }
}
