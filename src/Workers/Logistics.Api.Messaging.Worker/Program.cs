using Logistics.Api.BuildingBlocks.Contracts;
using Logistics.Api.Messaging.Worker.Services;
using Logistics.Api.Notifications.Infrastructure;
using Logistics.Api.Notifications.Infrastructure.Messaging;
using Logistics.Api.Notifications.Infrastructure.Persistence;
using Logistics.Api.Notifications.Infrastructure.Services;
using Logistics.Api.Shipments.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;

var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console())
    .ConfigureServices((context, services) =>
    {
        services.AddNotificationsModule(context.Configuration);
        services.AddDbContext<ShipmentsDbContext>(opts =>
            opts.UseNpgsql(
                context.Configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_shipments", "shipments")));

        services.Configure<WebhookDeliveryWorkerOptions>(
            context.Configuration.GetSection(WebhookDeliveryWorkerOptions.SectionName));
        services.Configure<OutboxPublisherOptions>(
            context.Configuration.GetSection(OutboxPublisherOptions.SectionName));

        services.AddHttpClient("webhook-delivery", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Logistics-Webhook-Worker/1.0");
        });

        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumer<ShipmentCreatedIntegrationEventConsumer>();
            cfg.AddConsumer<ShipmentStatusChangedIntegrationEventConsumer>();

            cfg.UsingRabbitMq((busContext, rabbit) =>
            {
                rabbit.Host(new Uri(context.Configuration.GetConnectionString("RabbitMQ")!));

                rabbit.ReceiveEndpoint("notifications.shipment-created", endpoint =>
                {
                    endpoint.UseConsumeFilter(typeof(InboxIdempotencyFilter<>), busContext);
                    endpoint.ConfigureConsumer<ShipmentCreatedIntegrationEventConsumer>(busContext);
                });

                rabbit.ReceiveEndpoint("notifications.shipment-status-changed", endpoint =>
                {
                    endpoint.UseConsumeFilter(typeof(InboxIdempotencyFilter<>), busContext);
                    endpoint.ConfigureConsumer<ShipmentStatusChangedIntegrationEventConsumer>(busContext);
                });
            });
        });

        services.AddHostedService<OutboxPublisherBackgroundService>();
        services.AddHostedService<WebhookDeliveryBackgroundService>();
    })
    .Build();

await host.RunAsync();