using Logistics.Api.BuildingBlocks.Domain.Time;
using Logistics.Api.Identity.Application;
using Logistics.Api.Identity.Application.Abstractions;
using Logistics.Api.Identity.Domain.Repositories;
using Logistics.Api.Identity.Infrastructure.Persistence;
using Logistics.Api.Identity.Infrastructure.Repositories;
using Logistics.Api.Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Api.Identity.Infrastructure;

/// <summary>
/// Registers all Infrastructure services for the Identity module.
/// Call <c>services.AddIdentityModule(configuration)</c> from the Host.
/// </summary>
public static class IdentityInfrastructureModule
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Application services (MediatR handlers, validators)
        services.AddIdentityApplicationServices();

        // EF Core – Identity DbContext
        services.AddDbContext<IdentityDbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_identity", "identity")));

        // Domain clock
        services.AddSingleton<IClock, SystemClock>();

        // JWT
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                         ?? throw new InvalidOperationException($"Missing configuration section '{JwtOptions.SectionName}'.");
        services.AddSingleton(jwtOptions);
        services.AddSingleton<IJwtService, JwtService>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
