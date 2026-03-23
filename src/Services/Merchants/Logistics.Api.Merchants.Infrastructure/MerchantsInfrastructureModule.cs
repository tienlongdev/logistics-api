using Logistics.Api.Merchants.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Api.Merchants.Infrastructure;

/// <summary>
/// Registers all Infrastructure services for the Merchants module.
/// Call <c>services.AddMerchantsModule(configuration)</c> from the Host.
/// </summary>
public static class MerchantsInfrastructureModule
{
    public static IServiceCollection AddMerchantsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<MerchantsDbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_merchants", "merchants")));

        return services;
    }
}
