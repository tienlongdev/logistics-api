namespace Logistics.Api.Search.Application.Common;

public static class ShipmentSearchSortParser
{
    private static readonly IReadOnlySet<string> AllowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "createdAt",
        "updatedAt",
        "trackingCode",
        "shipmentCode",
        "merchantCode",
        "receiverPhone",
        "status"
    };

    public static (string Field, string Order) Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return ("createdAt", "desc");

        var parts = raw.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var field = parts.Length > 0 ? parts[0] : "createdAt";
        var order = parts.Length > 1 ? parts[1] : "desc";

        if (!AllowedFields.Contains(field))
            field = "createdAt";

        order = order.Equals("asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
        return (field, order);
    }

    public static bool IsValid(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return true;

        var parts = raw.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length is 0 or > 2)
            return false;

        if (!AllowedFields.Contains(parts[0]))
            return false;

        return parts.Length == 1 ||
               parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase) ||
               parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
    }
}