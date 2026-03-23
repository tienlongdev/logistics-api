using Logistics.Api.BuildingBlocks.Domain.Abstractions;

namespace Logistics.Api.Identity.Domain.Entities;

/// <summary>
/// System role (Admin, Operator, HubStaff, Merchant).
/// </summary>
public sealed class Role : Entity<Guid>
{
    // Required by EF Core
    private Role() { }

    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    public static class Names
    {
        public const string Admin = "Admin";
        public const string Operator = "Operator";
        public const string HubStaff = "HubStaff";
        public const string Merchant = "Merchant";
    }
}
