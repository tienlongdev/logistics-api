using Logistics.Api.Notifications.Application;
using Logistics.Api.Notifications.Application.Abstractions;
using Logistics.Api.Notifications.Infrastructure.Persistence;
using Logistics.Api.Notifications.Infrastructure.Repositories;
using Logistics.Api.Notifications.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Api.Notifications.Infrastructure;

public static class NotificationsInfrastructureModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddNotificationsApplicationServices();

        services.AddDbContext<NotificationsDbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_notifications", "notifications")));

        services.AddScoped<IMerchantScopeService, MerchantScopeService>();
        services.AddScoped<IWebhookSubscriptionRepository, WebhookSubscriptionRepository>();

        return services;
    }
}