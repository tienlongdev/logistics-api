using Logistics.Api.Shipments.Domain.Enums;

namespace Logistics.Api.Shipments.Domain.Services;

/// <summary>
/// Defines allowed status transitions for the Shipment aggregate.
///
/// State machine diagram:
///   Created         → AwaitingPickup | Cancelled
///   AwaitingPickup  → PickedUp | Cancelled
///   PickedUp        → InTransit
///   InTransit       → OutForDelivery | Returning
///   OutForDelivery  → Delivered | DeliveryFailed
///   DeliveryFailed  → OutForDelivery | Returning
///   Returning       → Returned
///   [Terminal: Delivered, Returned, Cancelled] → (no transitions)
/// </summary>
public static class ShipmentStateMachine
{
    private static readonly IReadOnlyDictionary<ShipmentStatus, IReadOnlySet<ShipmentStatus>> AllowedTransitions =
        new Dictionary<ShipmentStatus, IReadOnlySet<ShipmentStatus>>
        {
            [ShipmentStatus.Created] = new HashSet<ShipmentStatus> { ShipmentStatus.AwaitingPickup, ShipmentStatus.Cancelled },
            [ShipmentStatus.AwaitingPickup] = new HashSet<ShipmentStatus> { ShipmentStatus.PickedUp, ShipmentStatus.Cancelled },
            [ShipmentStatus.PickedUp] = new HashSet<ShipmentStatus> { ShipmentStatus.InTransit },
            [ShipmentStatus.InTransit] = new HashSet<ShipmentStatus> { ShipmentStatus.OutForDelivery, ShipmentStatus.Returning },
            [ShipmentStatus.OutForDelivery] = new HashSet<ShipmentStatus> { ShipmentStatus.Delivered, ShipmentStatus.DeliveryFailed },
            [ShipmentStatus.DeliveryFailed] = new HashSet<ShipmentStatus> { ShipmentStatus.OutForDelivery, ShipmentStatus.Returning },
            [ShipmentStatus.Returning] = new HashSet<ShipmentStatus> { ShipmentStatus.Returned },
            [ShipmentStatus.Returned] = new HashSet<ShipmentStatus>(),
            [ShipmentStatus.Delivered] = new HashSet<ShipmentStatus>(),
            [ShipmentStatus.Cancelled] = new HashSet<ShipmentStatus>(),
        };

    /// <summary>Returns true if the transition from <paramref name="from"/> to <paramref name="to"/> is allowed.</summary>
    public static bool CanTransition(ShipmentStatus from, ShipmentStatus to)
        => AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    /// <summary>Returns the set of statuses reachable from <paramref name="from"/>.</summary>
    public static IReadOnlySet<ShipmentStatus> GetAllowedTransitions(ShipmentStatus from)
        => AllowedTransitions.TryGetValue(from, out var allowed)
            ? allowed
            : (IReadOnlySet<ShipmentStatus>)new HashSet<ShipmentStatus>();

    /// <summary>Returns true if <paramref name="status"/> is a terminal state (no further transitions).</summary>
    public static bool IsTerminal(ShipmentStatus status)
        => AllowedTransitions.TryGetValue(status, out var allowed) && allowed.Count == 0;
}
