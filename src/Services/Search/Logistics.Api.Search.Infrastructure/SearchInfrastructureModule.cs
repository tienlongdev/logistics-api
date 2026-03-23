using Logistics.Api.Search.Application;
using Logistics.Api.Search.Application.Abstractions;
using Logistics.Api.Search.Infrastructure.Options;
using Logistics.Api.Search.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Api.Search.Infrastructure;

public static class SearchInfrastructureModule
{
    public static IServiceCollection AddSearchModule(
        this IServiceCollection services,
        IConfiguration configuration,
        bool registerHostedServices = false)
    {
        services.AddSearchApplicationServices();
        services.Configure<ShipmentSearchOptions>(configuration.GetSection(ShipmentSearchOptions.SectionName));

        var elasticsearchConnectionString = configuration.GetConnectionString("Elasticsearch")
            ?? throw new InvalidOperationException("Missing Elasticsearch connection string.");

        services.AddHttpClient("elasticsearch", client =>
        {
            client.BaseAddress = new Uri(elasticsearchConnectionString.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddScoped<IShipmentSearchReadService, ElasticsearchShipmentSearchService>();
        services.AddScoped<IShipmentSearchIndexService, ElasticsearchShipmentSearchService>();

        if (registerHostedServices)
            services.AddHostedService<ShipmentSearchIndexInitializer>();

        return services;
    }
}