using Logistics.Api.BuildingBlocks.Contracts;
using Logistics.Api.Messaging.Worker.Services;
using Logistics.Api.Search.Infrastructure;
using Logistics.Api.Search.Infrastructure.Messaging;
using Logistics.Api.Shipments.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console())
    .ConfigureServices((context, services) =>
    {
        services.AddSearchModule(context.Configuration, registerHostedServices: true);
        services.AddDbContext<ShipmentsDbContext>(opts =>
            opts.UseNpgsql(
                context.Configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_shipments", "shipments")));

        services.Configure<OutboxPublisherOptions>(
            context.Configuration.GetSection(OutboxPublisherOptions.SectionName));

        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumer<ShipmentCreatedSearchIndexConsumer>();
            cfg.AddConsumer<ShipmentStatusChangedSearchIndexConsumer>();

            cfg.UsingRabbitMq((busContext, rabbit) =>
            {
                rabbit.Host(new Uri(context.Configuration.GetConnectionString("RabbitMQ")!));

                rabbit.ReceiveEndpoint("search.shipment-created", endpoint =>
                {
                    endpoint.ConfigureConsumer<ShipmentCreatedSearchIndexConsumer>(busContext);
                });

                rabbit.ReceiveEndpoint("search.shipment-status-changed", endpoint =>
                {
                    endpoint.ConfigureConsumer<ShipmentStatusChangedSearchIndexConsumer>(busContext);
                });
            });
        });

        services.AddHostedService<OutboxPublisherBackgroundService>();
    })
    .Build();

await host.RunAsync();