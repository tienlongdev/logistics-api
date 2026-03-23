using FluentAssertions;
using Logistics.Api.Shipments.Domain.Enums;
using Logistics.Api.Shipments.Domain.Services;

namespace Logistics.Api.UnitTests.Shipments;

public sealed class ShipmentStateMachineTests
{
    // ── CanTransition — valid paths ───────────────────────────────────────────

    [Theory]
    [InlineData(ShipmentStatus.Created, ShipmentStatus.AwaitingPickup)]
    [InlineData(ShipmentStatus.Created, ShipmentStatus.Cancelled)]
    [InlineData(ShipmentStatus.AwaitingPickup, ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.AwaitingPickup, ShipmentStatus.Cancelled)]
    [InlineData(ShipmentStatus.PickedUp, ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.InTransit, ShipmentStatus.OutForDelivery)]
    [InlineData(ShipmentStatus.InTransit, ShipmentStatus.Returning)]
    [InlineData(ShipmentStatus.OutForDelivery, ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.OutForDelivery, ShipmentStatus.DeliveryFailed)]
    [InlineData(ShipmentStatus.DeliveryFailed, ShipmentStatus.OutForDelivery)]
    [InlineData(ShipmentStatus.DeliveryFailed, ShipmentStatus.Returning)]
    [InlineData(ShipmentStatus.Returning, ShipmentStatus.Returned)]
    public void CanTransition_ReturnsTrue_ForAllAllowedTransitions(
        ShipmentStatus from, ShipmentStatus to)
    {
        ShipmentStateMachine.CanTransition(from, to).Should().BeTrue(
            because: $"{from} → {to} is an allowed transition");
    }

    // ── CanTransition — invalid paths ─────────────────────────────────────────

    [Theory]
    [InlineData(ShipmentStatus.Created, ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.Created, ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.Created, ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.Created, ShipmentStatus.Returned)]
    [InlineData(ShipmentStatus.AwaitingPickup, ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.AwaitingPickup, ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.PickedUp, ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.PickedUp, ShipmentStatus.Cancelled)]
    [InlineData(ShipmentStatus.PickedUp, ShipmentStatus.AwaitingPickup)]
    [InlineData(ShipmentStatus.InTransit, ShipmentStatus.Created)]
    [InlineData(ShipmentStatus.InTransit, ShipmentStatus.Cancelled)]
    [InlineData(ShipmentStatus.OutForDelivery, ShipmentStatus.Created)]
    [InlineData(ShipmentStatus.OutForDelivery, ShipmentStatus.Returning)]
    [InlineData(ShipmentStatus.DeliveryFailed, ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.DeliveryFailed, ShipmentStatus.Cancelled)]
    [InlineData(ShipmentStatus.Returning, ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.Returning, ShipmentStatus.Cancelled)]
    public void CanTransition_ReturnsFalse_ForDisallowedTransitions(
        ShipmentStatus from, ShipmentStatus to)
    {
        ShipmentStateMachine.CanTransition(from, to).Should().BeFalse(
            because: $"{from} → {to} is not an allowed transition");
    }

    // ── Terminal states ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.Returned)]
    [InlineData(ShipmentStatus.Cancelled)]
    public void IsTerminal_ReturnsTrue_ForTerminalStatuses(ShipmentStatus status)
    {
        ShipmentStateMachine.IsTerminal(status).Should().BeTrue(
            because: $"{status} is a terminal state");
    }

    [Theory]
    [InlineData(ShipmentStatus.Created)]
    [InlineData(ShipmentStatus.AwaitingPickup)]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.OutForDelivery)]
    [InlineData(ShipmentStatus.DeliveryFailed)]
    [InlineData(ShipmentStatus.Returning)]
    public void IsTerminal_ReturnsFalse_ForNonTerminalStatuses(ShipmentStatus status)
    {
        ShipmentStateMachine.IsTerminal(status).Should().BeFalse(
            because: $"{status} is not a terminal state");
    }

    // ── CanTransition — self-transitions always invalid ───────────────────────

    [Theory]
    [InlineData(ShipmentStatus.Created)]
    [InlineData(ShipmentStatus.AwaitingPickup)]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.Cancelled)]
    public void CanTransition_ReturnsFalse_ForSelfTransitions(ShipmentStatus status)
    {
        ShipmentStateMachine.CanTransition(status, status).Should().BeFalse(
            because: "self-transitions are never allowed");
    }

    // ── Terminal states cannot transition anywhere ────────────────────────────

    [Theory]
    [InlineData(ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.Returned)]
    [InlineData(ShipmentStatus.Cancelled)]
    public void CanTransition_ReturnsFalse_FromAnyTerminalStatus(ShipmentStatus terminal)
    {
        foreach (ShipmentStatus target in Enum.GetValues<ShipmentStatus>())
        {
            ShipmentStateMachine.CanTransition(terminal, target).Should().BeFalse(
                because: $"terminal state {terminal} → {target} must always be false");
        }
    }

    // ── GetAllowedTransitions ─────────────────────────────────────────────────

    [Fact]
    public void GetAllowedTransitions_ForCreated_ReturnsTwoStatuses()
    {
        var allowed = ShipmentStateMachine.GetAllowedTransitions(ShipmentStatus.Created);

        allowed.Should().BeEquivalentTo(new[]
        {
            ShipmentStatus.AwaitingPickup,
            ShipmentStatus.Cancelled
        });
    }

    [Fact]
    public void GetAllowedTransitions_ForPickedUp_ReturnsOnlyInTransit()
    {
        var allowed = ShipmentStateMachine.GetAllowedTransitions(ShipmentStatus.PickedUp);

        allowed.Should().ContainSingle()
            .Which.Should().Be(ShipmentStatus.InTransit);
    }

    [Fact]
    public void GetAllowedTransitions_ForDelivered_ReturnsEmpty()
    {
        var allowed = ShipmentStateMachine.GetAllowedTransitions(ShipmentStatus.Delivered);

        allowed.Should().BeEmpty();
    }

    [Fact]
    public void GetAllowedTransitions_ForDeliveryFailed_ReturnsTwoStatuses()
    {
        var allowed = ShipmentStateMachine.GetAllowedTransitions(ShipmentStatus.DeliveryFailed);

        allowed.Should().BeEquivalentTo(new[]
        {
            ShipmentStatus.OutForDelivery,
            ShipmentStatus.Returning
        });
    }
}
