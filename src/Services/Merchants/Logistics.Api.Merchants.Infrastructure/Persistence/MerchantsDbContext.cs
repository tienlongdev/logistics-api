using Logistics.Api.Merchants.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Merchants.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the Merchants module.
/// Uses the "merchants" PostgreSQL schema (schema-per-module pattern).
/// </summary>
public sealed class MerchantsDbContext : DbContext
{
    public MerchantsDbContext(DbContextOptions<MerchantsDbContext> options) : base(options) { }

    internal DbSet<MerchantEntity> Merchants => Set<MerchantEntity>();
    internal DbSet<MerchantUserEntity> MerchantUsers => Set<MerchantUserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("merchants");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MerchantsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
