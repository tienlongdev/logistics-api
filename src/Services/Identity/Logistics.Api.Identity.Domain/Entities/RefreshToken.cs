using Logistics.Api.BuildingBlocks.Domain.Abstractions;
using Logistics.Api.BuildingBlocks.Domain.Time;

namespace Logistics.Api.Identity.Domain.Entities;

/// <summary>
/// Opaque refresh token (stored as SHA-256 hash — never plain text).
/// </summary>
public sealed class RefreshToken : Entity<Guid>
{
    // Required by EF Core
    private RefreshToken() { }

    public Guid UserId { get; private set; }

    /// <summary>SHA-256 hash of the raw opaque token sent to the client.</summary>
    public string TokenHash { get; private set; } = default!;

    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevokedReason { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        string? ipAddress,
        string? userAgent,
        IClock clock) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = clock.UtcNow
        };

    public void Revoke(string reason, IClock clock)
    {
        RevokedAt = clock.UtcNow;
        RevokedReason = reason;
    }
}
