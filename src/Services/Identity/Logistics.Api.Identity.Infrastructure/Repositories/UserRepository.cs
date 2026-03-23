using Logistics.Api.Identity.Domain.Entities;
using Logistics.Api.Identity.Domain.Repositories;
using Logistics.Api.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Identity.Infrastructure.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken ct = default) =>
        _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant().Trim(), ct);

    public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default) =>
        _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<RefreshToken?> GetActiveRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default) =>
        _context.RefreshTokens
            .FirstOrDefaultAsync(
                rt => rt.TokenHash == tokenHash
                      && rt.RevokedAt == null
                      && rt.ExpiresAt > DateTimeOffset.UtcNow,
                ct);

    public void Add(User user) => _context.Users.Add(user);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
}
