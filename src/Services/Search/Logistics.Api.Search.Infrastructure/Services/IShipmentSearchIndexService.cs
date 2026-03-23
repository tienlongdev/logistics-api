using Logistics.Api.Search.Infrastructure.Models;

namespace Logistics.Api.Search.Infrastructure.Services;

public interface IShipmentSearchIndexService
{
    Task EnsureIndexAsync(CancellationToken ct = default);
    Task UpsertShipmentAsync(ShipmentSearchDocument document, CancellationToken ct = default);
}