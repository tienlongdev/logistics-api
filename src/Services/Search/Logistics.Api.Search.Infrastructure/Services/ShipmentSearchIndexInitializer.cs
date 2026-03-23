using Logistics.Api.Search.Infrastructure.Options;
using Logistics.Api.Shipments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Api.Search.Infrastructure.Services;

public sealed class ShipmentSearchIndexInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ShipmentSearchOptions _options;
    private readonly ILogger<ShipmentSearchIndexInitializer> _logger;

    public ShipmentSearchIndexInitializer(
        IServiceScopeFactory scopeFactory,
        IOptions<ShipmentSearchOptions> options,
        ILogger<ShipmentSearchIndexInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.EnsureIndexOnStartup && !_options.BackfillOnStartup)
            return;

        using var scope = _scopeFactory.CreateScope();
        var indexService = scope.ServiceProvider.GetRequiredService<IShipmentSearchIndexService>();

        if (_options.EnsureIndexOnStartup)
            await indexService.EnsureIndexAsync(cancellationToken);

        if (!_options.BackfillOnStartup)
            return;

        var dbContext = scope.ServiceProvider.GetRequiredService<ShipmentsDbContext>();
        var totalIndexed = 0;
        var page = 0;

        while (true)
        {
            var shipments = await dbContext.Shipments
                .AsNoTracking()
                .OrderBy(x => x.CreatedAt)
                .Skip(page * _options.BackfillBatchSize)
                .Take(_options.BackfillBatchSize)
                .ToListAsync(cancellationToken);

            if (shipments.Count == 0)
                break;

            foreach (var shipment in shipments)
            {
                await indexService.UpsertShipmentAsync(Models.ShipmentSearchDocument.FromShipment(shipment), cancellationToken);
                totalIndexed += 1;
            }

            page += 1;
        }

        _logger.LogInformation("Backfilled {ShipmentCount} shipments into Elasticsearch index {IndexName}", totalIndexed, _options.IndexName);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}