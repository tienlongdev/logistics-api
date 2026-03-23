namespace Logistics.Api.Notifications.Application.Common;

public static class WebhookEventNames
{
    public const string ShipmentCreated = "ShipmentCreated";
    public const string ShipmentStatusChanged = "ShipmentStatusChanged";

    public static readonly IReadOnlySet<string> Supported = new HashSet<string>(StringComparer.Ordinal)
    {
        ShipmentCreated,
        ShipmentStatusChanged
    };
}