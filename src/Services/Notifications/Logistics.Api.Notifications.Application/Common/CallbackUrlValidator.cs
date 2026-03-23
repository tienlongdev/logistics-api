namespace Logistics.Api.Notifications.Application.Common;

public static class CallbackUrlValidator
{
    public static bool IsValid(string? callbackUrl)
    {
        if (string.IsNullOrWhiteSpace(callbackUrl))
            return false;

        if (!Uri.TryCreate(callbackUrl, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return true;

        return uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
               (uri.IsLoopback ||
                uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase));
    }
}