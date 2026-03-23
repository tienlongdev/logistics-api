namespace Logistics.Api.Identity.Application.Abstractions;

/// <summary>
/// Service interface for JWT token generation and token hashing.
/// Implemented in the Infrastructure layer.
/// </summary>
public interface IJwtService
{
    /// <summary>Generate a signed JWT access token for the given user.</summary>
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);

    /// <summary>Generate a cryptographically random opaque refresh token (raw, to be sent to client).</summary>
    string GenerateRefreshTokenRaw();

    /// <summary>Compute a SHA-256 hex hash of the given raw token for safe storage.</summary>
    string HashToken(string rawToken);

    /// <summary>Access token lifetime in seconds (from JwtOptions).</summary>
    int AccessTokenExpiresInSeconds { get; }

    /// <summary>Refresh token lifetime in days (from JwtOptions).</summary>
    int RefreshTokenExpiresInDays { get; }
}
