using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Shipments.Domain.Enums;

namespace Logistics.Api.Shipments.Application.Errors;

public static class ShipmentErrors
{
    public static readonly Error NotFound =
        new("shipments.not_found", "Không tìm thấy đơn hàng.");

    public static readonly Error MerchantScopeForbidden =
        new("shipments.merchant_scope_forbidden",
            "Người dùng không thuộc về bất kỳ merchant nào. Vui lòng liên hệ quản trị viên.");

    public static readonly Error NoPricingRule =
        new("shipments.no_pricing_rule",
            "Không tìm thấy quy tắc định giá phù hợp cho đơn hàng này.");

    public static Error InvalidStatusTransition(ShipmentStatus from, ShipmentStatus to) =>
        new("shipments.invalid_state_transition",
            $"Không thể chuyển trạng thái từ '{from}' sang '{to}'.");
}
