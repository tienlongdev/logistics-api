using Logistics.Api.Shipments.Infrastructure.Messaging;
using Logistics.Api.Shipments.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var postgres = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Missing Postgres connection string.");
var rabbitMq = builder.Configuration.GetConnectionString("RabbitMQ")
    ?? throw new InvalidOperationException("Missing RabbitMQ connection string.");

builder.Services.Configure<MessagingWorkerOptions>(
    builder.Configuration.GetSection(MessagingWorkerOptions.SectionName));

builder.Services.AddDbContext<ShipmentsDbContext>(opts =>
    opts.UseNpgsql(
        postgres,
        npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_shipments", "shipments")));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ShipmentCreatedIntegrationEventConsumer>();
    x.AddConsumer<ShipmentStatusChangedIntegrationEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(rabbitMq));
        cfg.UseConsumeFilter(typeof(InboxIdempotencyFilter<>), context);
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHostedService<OutboxPublisherBackgroundService>();

var host = builder.Build();
await host.RunAsync();