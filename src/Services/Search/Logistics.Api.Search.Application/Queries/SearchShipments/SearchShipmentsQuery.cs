using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;

namespace Logistics.Api.Search.Application.Queries.SearchShipments;

public sealed record SearchShipmentsQuery(
    string? TrackingCode,
    string? ShipmentCode,
    string? ReceiverPhone,
    string? MerchantCode,
    string? Status,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int Page,
    int PageSize,
    string? Sort)
    : IQuery<Result<SearchShipmentsResponse>>;