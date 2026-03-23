using Logistics.Api.BuildingBlocks.Application.Results;

namespace Logistics.Api.Pricing.Application.Errors;

/// <summary>Stable error codes for the Pricing module.</summary>
public static class PricingErrors
{
    public static readonly Error NoPricingRuleFound =
        new("pricing.no_rule_found",
            "Không tìm thấy bảng giá phù hợp cho tuyến đường và loại dịch vụ này.");
}
