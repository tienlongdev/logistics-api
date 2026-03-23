using Logistics.Api.Identity.Domain.Entities;

namespace Logistics.Api.Identity.Domain.Repositories;

/// <summary>
/// Repository contract for the Identity domain.
/// Implementations live in the Infrastructure layer.
/// </summary>
public interface IUserRepository
{
    /// <summary>Find user by normalised email, including UserRoles and their Role details.</summary>
    Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken ct = default);

    /// <summary>Find user by id, including UserRoles and their Role details.</summary>
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Find an active (non-revoked, non-expired) refresh token by its hash.
    /// Returns null if not found or already inactive.
    /// </summary>
    Task<RefreshToken?> GetActiveRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default);

    void Add(User user);

    Task SaveChangesAsync(CancellationToken ct = default);
}
