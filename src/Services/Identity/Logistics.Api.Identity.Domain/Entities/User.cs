using Logistics.Api.BuildingBlocks.Domain.Abstractions;
using Logistics.Api.BuildingBlocks.Domain.Time;

namespace Logistics.Api.Identity.Domain.Entities;

/// <summary>
/// User aggregate root. Manages its own refresh tokens.
/// Password verification is intentionally left to the Application layer (BCrypt).
/// </summary>
public sealed class User : AggregateRoot<Guid>
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    // Required by EF Core
    private User() { }

    public string Email { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string FullName { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public bool EmailVerified { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public static User Create(
        string email,
        string fullName,
        string passwordHash,
        IClock clock)
    {
        var now = clock.UtcNow;
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant().Trim(),
            FullName = fullName.Trim(),
            PasswordHash = passwordHash,
            IsActive = true,
            EmailVerified = false,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Creates and registers a new refresh token for this user.
    /// The returned token should be persisted via the repository.
    /// </summary>
    public RefreshToken AddRefreshToken(
        string tokenHash,
        DateTimeOffset expiresAt,
        string? ipAddress,
        string? userAgent,
        IClock clock)
    {
        var token = RefreshToken.Create(Id, tokenHash, expiresAt, ipAddress, userAgent, clock);
        _refreshTokens.Add(token);
        UpdatedAt = clock.UtcNow;
        return token;
    }
}
