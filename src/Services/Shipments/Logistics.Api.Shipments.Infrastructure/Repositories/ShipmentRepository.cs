using System.Data;
using Dapper;
using Logistics.Api.Shipments.Domain.Entities;
using Logistics.Api.Shipments.Domain.Repositories;
using Logistics.Api.Shipments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Shipments.Infrastructure.Repositories;

internal sealed class ShipmentRepository : IShipmentRepository
{
    private readonly ShipmentsDbContext _context;

    public ShipmentRepository(ShipmentsDbContext context)
    {
        _context = context;
    }

    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Shipments
            .Include(s => s.TrackingEvents)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<Shipment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default) =>
        _context.Shipments
            .FirstOrDefaultAsync(s => s.IdempotencyKey == idempotencyKey, ct);

    public async Task<long> GetNextSequenceNumberAsync(CancellationToken ct = default)
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State == ConnectionState.Closed)
            await ((System.Data.Common.DbConnection)conn).OpenAsync(ct);

        return await conn.QuerySingleAsync<long>(
            "SELECT nextval('shipments.shipment_number_seq')");
    }

    public void Add(Shipment shipment) => _context.Shipments.Add(shipment);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
}
