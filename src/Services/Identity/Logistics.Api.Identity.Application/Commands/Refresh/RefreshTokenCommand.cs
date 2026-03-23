using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Identity.Application.Dtos;

namespace Logistics.Api.Identity.Application.Commands.Refresh;

/// <summary>
/// Command to rotate a refresh token and issue a new access + refresh token pair.
/// </summary>
public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress,
    string? UserAgent) : ICommand<Result<AuthResponse>>;
