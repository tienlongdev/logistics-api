using Logistics.Api.Pricing.Application;
using Logistics.Api.Pricing.Domain.Repositories;
using Logistics.Api.Pricing.Infrastructure.Persistence;
using Logistics.Api.Pricing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Api.Pricing.Infrastructure;

/// <summary>
/// Registers all Infrastructure services for the Pricing module.
/// Call <c>services.AddPricingModule(configuration)</c> from the Host.
/// </summary>
public static class PricingInfrastructureModule
{
    public static IServiceCollection AddPricingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Application services (IPricingCalculator etc.)
        services.AddPricingApplicationServices();

        // EF Core – Pricing DbContext
        services.AddDbContext<PricingDbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_pricing", "pricing")));

        // Repositories
        services.AddScoped<IPricingRuleRepository, PricingRuleRepository>();

        return services;
    }
}
