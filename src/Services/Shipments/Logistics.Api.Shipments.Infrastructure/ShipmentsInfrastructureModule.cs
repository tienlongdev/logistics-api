using Logistics.Api.BuildingBlocks.Domain.Time;
using Logistics.Api.Shipments.Application;
using Logistics.Api.Shipments.Application.Abstractions;
using Logistics.Api.Shipments.Domain.Repositories;
using Logistics.Api.Shipments.Infrastructure.Idempotency;
using Logistics.Api.Shipments.Infrastructure.Persistence;
using Logistics.Api.Shipments.Infrastructure.Repositories;
using Logistics.Api.Shipments.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Api.Shipments.Infrastructure;

/// <summary>
/// Registers all Infrastructure services for the Shipments module.
/// Call <c>services.AddShipmentsModule(configuration)</c> from the Host.
/// </summary>
public static class ShipmentsInfrastructureModule
{
    public static IServiceCollection AddShipmentsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Application services
        services.AddShipmentsApplicationServices();

        // EF Core – Shipments DbContext
        services.AddDbContext<ShipmentsDbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_shipments", "shipments")));

        // Domain clock (shared with other modules via Singleton — safe since SystemClock is stateless)
        services.AddSingleton<IClock, SystemClock>();

        // Repositories
        services.AddScoped<IShipmentRepository, ShipmentRepository>();

        // Cross-module merchant scope resolver
        services.AddScoped<IMerchantScopeService, MerchantScopeService>();

        // A4: idempotency store (Redis via IDistributedCache — registered in Host)
        services.AddScoped<IIdempotencyService, RedisIdempotencyService>();

        return services;
    }
}
