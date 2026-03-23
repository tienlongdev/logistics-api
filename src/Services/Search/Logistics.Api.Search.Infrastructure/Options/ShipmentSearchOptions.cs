namespace Logistics.Api.Search.Infrastructure.Options;

public sealed class ShipmentSearchOptions
{
    public const string SectionName = "Search:Shipments";

    public string IndexName { get; init; } = "logistics-shipments-v1";
    public bool EnsureIndexOnStartup { get; init; } = false;
    public bool BackfillOnStartup { get; init; } = false;
    public int BackfillBatchSize { get; init; } = 500;
}