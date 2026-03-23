using Logistics.Api.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Identity.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the Identity module.
/// Uses the "identity" PostgreSQL schema (schema-per-module pattern).
/// </summary>
public sealed class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
