using Logistics.Api.BuildingBlocks.Application.Results;

namespace Logistics.Api.Notifications.Application.Errors;

public static class NotificationErrors
{
    public static readonly Error NotFound =
        new("notifications.not_found", "Không tìm thấy webhook subscription.");

    public static readonly Error MerchantScopeForbidden =
        new("notifications.merchant_scope_forbidden", "Người dùng không thuộc merchant nào.");
}