using System.ComponentModel.DataAnnotations;

namespace Logistics.Api.Identity.Infrastructure.Services;

/// <summary>
/// JWT configuration bound from appsettings.json: "Jwt" section.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = default!;

    [Required]
    public string Audience { get; set; } = default!;

    /// <summary>
    /// HS256 signing key. Must be at least 32 bytes (256 bits).
    /// Never log or expose this value.
    /// </summary>
    [Required]
    public string SecretKey { get; set; } = default!;

    public int AccessTokenExpiresInSeconds { get; set; } = 3600;

    public int RefreshTokenExpiresInDays { get; set; } = 30;
}
