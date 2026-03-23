using FluentValidation;
using Logistics.Api.Search.Application.Common;

namespace Logistics.Api.Search.Application.Queries.SearchShipments;

public sealed class SearchShipmentsQueryValidator : AbstractValidator<SearchShipmentsQuery>
{
    public SearchShipmentsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Sort)
            .Must(ShipmentSearchSortParser.IsValid)
            .WithMessage("Sort phải có dạng field:asc|desc và thuộc danh sách được hỗ trợ.");
        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate <= x.ToDate)
            .WithMessage("fromDate phải nhỏ hơn hoặc bằng toDate.");
    }
}