using Logistics.Api.Identity.Domain.Entities;
using Logistics.Api.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Identity.Infrastructure;

public static class IdentityDatabaseSeeder
{
    public static async Task MigrateAndSeedIdentityAsync(this IServiceProvider services, IConfiguration configuration, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("IdentityDatabaseSeeder");

        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        // Apply Identity migrations automatically so local clones can login without manual DB steps.
        await dbContext.Database.MigrateAsync(ct);

        var options = configuration.GetSection("SeedData:Identity").Get<IdentitySeedOptions>() ?? new IdentitySeedOptions();

        if (!options.Enabled)
        {
            logger.LogInformation("Identity seed is disabled by configuration.");
            return;
        }

        await SeedRolesAsync(dbContext, ct);
        await SeedUsersAsync(dbContext, logger, options.Users, ct);
    }

    private static async Task SeedRolesAsync(IdentityDbContext dbContext, CancellationToken ct)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO identity.roles (""Id"", ""Name"", ""Description"") VALUES
                ({Guid.Parse("00000000-0000-0000-0000-000000000001")}, {Role.Names.Admin}, {"System administrator"}),
                ({Guid.Parse("00000000-0000-0000-0000-000000000002")}, {Role.Names.Operator}, {"Operations staff"}),
                ({Guid.Parse("00000000-0000-0000-0000-000000000003")}, {Role.Names.HubStaff}, {"Hub warehouse staff"}),
                ({Guid.Parse("00000000-0000-0000-0000-000000000004")}, {Role.Names.Merchant}, {"Merchant / shipper"})
            ON CONFLICT (""Name"") DO NOTHING;
        ", ct);
    }

    private static async Task SeedUsersAsync(
        IdentityDbContext dbContext,
        ILogger logger,
        IReadOnlyList<IdentitySeedUser> users,
        CancellationToken ct)
    {
        if (users.Count == 0)
        {
            return;
        }

        var validRoles = new HashSet<string>
        {
            Role.Names.Admin,
            Role.Names.Operator,
            Role.Names.HubStaff,
            Role.Names.Merchant
        };

        foreach (var seedUser in users)
        {
            var normalizedEmail = seedUser.Email.Trim().ToLowerInvariant();
            var role = seedUser.Role.Trim();

            if (!validRoles.Contains(role))
            {
                logger.LogWarning("Skipping seeded user {Email}: unsupported role {Role}.", normalizedEmail, role);
                continue;
            }

            var exists = await dbContext.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
            if (!exists)
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(seedUser.Password, workFactor: 12);
                var now = DateTimeOffset.UtcNow;

                await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                    INSERT INTO identity.users
                        (""Id"", ""Email"", ""Phone"", ""FullName"", ""PasswordHash"", ""IsActive"", ""EmailVerified"", ""CreatedAt"", ""UpdatedAt"")
                    VALUES
                        ({Guid.NewGuid()}, {normalizedEmail}, {null as string}, {seedUser.FullName.Trim()}, {passwordHash}, {true}, {true}, {now}, {now})
                    ON CONFLICT (""Email"") DO NOTHING;
                ", ct);

                logger.LogInformation("Seeded identity user {Email} with role {Role}.", normalizedEmail, role);
            }

            await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO identity.user_roles (""Id"", ""UserId"", ""RoleId"", ""GrantedAt"")
                SELECT {Guid.NewGuid()}, u.""Id"", r.""Id"", {DateTimeOffset.UtcNow}
                FROM identity.users u
                INNER JOIN identity.roles r ON r.""Name"" = {role}
                WHERE u.""Email"" = {normalizedEmail}
                ON CONFLICT (""UserId"", ""RoleId"") DO NOTHING;
            ", ct);
        }
    }

    private sealed class IdentitySeedOptions
    {
        public bool Enabled { get; init; } = true;
        public IReadOnlyList<IdentitySeedUser> Users { get; init; } =
        [
            new IdentitySeedUser("merchant@demo.com", "P@ssw0rd!", "Demo Merchant", Role.Names.Merchant),
            new IdentitySeedUser("admin@demo.com", "P@ssw0rd!", "Demo Admin", Role.Names.Admin)
        ];
    }

    private sealed record IdentitySeedUser(string Email, string Password, string FullName, string Role);
}
