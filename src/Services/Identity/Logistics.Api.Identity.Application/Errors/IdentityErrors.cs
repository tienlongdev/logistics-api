using Logistics.Api.BuildingBlocks.Application.Results;

namespace Logistics.Api.Identity.Application.Errors;

/// <summary>
/// Stable error codes for the Identity module.
/// Format: "identity.&lt;snake_case&gt;"
/// </summary>
public static class IdentityErrors
{
    /// <summary>
    /// Used for both "user not found" and "wrong password" to prevent user-enumeration attacks.
    /// </summary>
    public static readonly Error InvalidCredentials =
        new("identity.invalid_credentials", "Email hoặc mật khẩu không đúng.");

    public static readonly Error UserInactive =
        new("identity.user_inactive", "Tài khoản đã bị vô hiệu hóa.");

    public static readonly Error InvalidRefreshToken =
        new("identity.invalid_refresh_token", "Refresh token không hợp lệ hoặc đã hết hạn.");
}
