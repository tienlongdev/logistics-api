namespace Logistics.Api.Search.Application.Queries.SearchShipments;

public sealed record SearchShipmentsResponse(
    long Total,
    int Page,
    int PageSize,
    string Sort,
    IReadOnlyList<SearchShipmentResponseItem> Items);

public sealed record SearchShipmentResponseItem(
    Guid ShipmentId,
    string TrackingCode,
    string ShipmentCode,
    string MerchantCode,
    string ReceiverPhone,
    string ReceiverName,
    string SenderName,
    string Status,
    string ServiceType,
    decimal CodAmount,
    decimal ShippingFee,
    decimal TotalFee,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);