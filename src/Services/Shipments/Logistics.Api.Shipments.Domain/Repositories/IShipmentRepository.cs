using Logistics.Api.Shipments.Domain.Entities;

namespace Logistics.Api.Shipments.Domain.Repositories;

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Shipment?> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default);

    Task<Shipment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);

    /// <summary>Returns the next globally unique sequence number used to generate codes.</summary>
    Task<long> GetNextSequenceNumberAsync(CancellationToken ct = default);

    void Add(Shipment shipment);

    Task SaveChangesAsync(CancellationToken ct = default);
}
