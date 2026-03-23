namespace Logistics.Api.Shipments.Application.Abstractions;

/// <summary>
/// Idempotency store: maps an Idempotency-Key to the shipment ID that was
/// created with that key. Backed by Redis with DB unique constraint as fallback.
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Returns the shipment ID associated with <paramref name="key"/>,
    /// or <c>null</c> if not found.
    /// </summary>
    Task<Guid?> TryGetAsync(string key, CancellationToken ct = default);

    /// <summary>Persists the association of <paramref name="key"/> → <paramref name="shipmentId"/>.</summary>
    Task StoreAsync(string key, Guid shipmentId, CancellationToken ct = default);
}
