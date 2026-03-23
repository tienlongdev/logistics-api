using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;

namespace Logistics.Api.Identity.Application.Commands.Logout;

/// <summary>
/// Command to revoke a refresh token (logout).
/// Idempotent: succeeds even if the token is already revoked.
/// </summary>
public sealed record LogoutCommand(string RefreshToken) : ICommand<Result>;
