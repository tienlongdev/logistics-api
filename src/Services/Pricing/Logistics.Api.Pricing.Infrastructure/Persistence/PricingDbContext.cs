using Logistics.Api.Pricing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Pricing.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the Pricing module.
/// Uses the "pricing" PostgreSQL schema (schema-per-module pattern).
/// </summary>
public sealed class PricingDbContext : DbContext
{
    public PricingDbContext(DbContextOptions<PricingDbContext> options) : base(options) { }

    public DbSet<PricingRule> PricingRules => Set<PricingRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("pricing");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PricingDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
