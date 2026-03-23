using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Search.Application.Abstractions;
using Logistics.Api.Search.Application.Common;

namespace Logistics.Api.Search.Application.Queries.SearchShipments;

internal sealed class SearchShipmentsQueryHandler
    : IQueryHandler<SearchShipmentsQuery, Result<SearchShipmentsResponse>>
{
    private readonly IShipmentSearchReadService _shipmentSearchReadService;

    public SearchShipmentsQueryHandler(IShipmentSearchReadService shipmentSearchReadService)
    {
        _shipmentSearchReadService = shipmentSearchReadService;
    }

    public async Task<Result<SearchShipmentsResponse>> Handle(
        SearchShipmentsQuery query,
        CancellationToken cancellationToken)
    {
        var (sortField, sortOrder) = ShipmentSearchSortParser.Parse(query.Sort);

        var result = await _shipmentSearchReadService.SearchAsync(
            new SearchShipmentsCriteria(
                query.TrackingCode,
                query.ShipmentCode,
                query.ReceiverPhone,
                query.MerchantCode,
                query.Status,
                query.FromDate,
                query.ToDate,
                query.Page,
                query.PageSize,
                sortField,
                sortOrder),
            cancellationToken);

        var items = result.Items
            .Select(x => new SearchShipmentResponseItem(
                x.ShipmentId,
                x.TrackingCode,
                x.ShipmentCode,
                x.MerchantCode,
                x.ReceiverPhone,
                x.ReceiverName,
                x.SenderName,
                x.Status,
                x.ServiceType,
                x.CodAmount,
                x.ShippingFee,
                x.TotalFee,
                x.CreatedAt,
                x.UpdatedAt))
            .ToArray();

        return Result<SearchShipmentsResponse>.Success(new SearchShipmentsResponse(
            result.Total,
            query.Page,
            query.PageSize,
            $"{sortField}:{sortOrder}",
            Array.AsReadOnly(items)));
    }
}