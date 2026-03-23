using Logistics.Api.Pricing.Application.Services;
using Logistics.Api.Pricing.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Api.Pricing.Application;

/// <summary>
/// Registers Application-layer services for the Pricing module.
/// Called from the Infrastructure installer.
/// </summary>
public static class PricingApplicationModule
{
    public static IServiceCollection AddPricingApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPricingCalculator, PricingCalculator>();
        return services;
    }
}
