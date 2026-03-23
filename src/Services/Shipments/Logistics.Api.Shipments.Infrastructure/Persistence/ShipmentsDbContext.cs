using Logistics.Api.Shipments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Shipments.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the Shipments module.
/// Uses the "shipments" PostgreSQL schema (schema-per-module pattern).
/// </summary>
public sealed class ShipmentsDbContext : DbContext
{
    public ShipmentsDbContext(DbContextOptions<ShipmentsDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<TrackingEvent> TrackingEvents => Set<TrackingEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("shipments");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShipmentsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
