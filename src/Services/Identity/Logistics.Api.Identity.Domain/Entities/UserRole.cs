using Logistics.Api.BuildingBlocks.Domain.Abstractions;

namespace Logistics.Api.Identity.Domain.Entities;

/// <summary>
/// Junction entity: maps User to Role with a granted timestamp.
/// </summary>
public sealed class UserRole : Entity<Guid>
{
    // Required by EF Core
    private UserRole() { }

    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTimeOffset GrantedAt { get; private set; }

    // Navigation
    public Role Role { get; private set; } = default!;

    public static UserRole Create(Guid userId, Guid roleId, DateTimeOffset grantedAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            GrantedAt = grantedAt
        };
}
