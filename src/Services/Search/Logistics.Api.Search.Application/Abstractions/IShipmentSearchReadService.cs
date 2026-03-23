namespace Logistics.Api.Search.Application.Abstractions;

public sealed record SearchShipmentsCriteria(
    string? TrackingCode,
    string? ShipmentCode,
    string? ReceiverPhone,
    string? MerchantCode,
    string? Status,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int Page,
    int PageSize,
    string SortField,
    string SortOrder);

public sealed record SearchShipmentsResult(
    long Total,
    IReadOnlyList<SearchShipmentDocumentDto> Items);

public sealed record SearchShipmentDocumentDto(
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

public interface IShipmentSearchReadService
{
    Task<SearchShipmentsResult> SearchAsync(SearchShipmentsCriteria criteria, CancellationToken ct = default);
}