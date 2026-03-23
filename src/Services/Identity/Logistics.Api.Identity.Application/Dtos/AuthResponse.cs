namespace Logistics.Api.Identity.Application.Dtos;

/// <summary>Response DTO returned by login and refresh commands.</summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn);
