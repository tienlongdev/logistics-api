using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Identity.Application.Dtos;

namespace Logistics.Api.Identity.Application.Commands.Login;

/// <summary>
/// Command to authenticate a user and issue JWT + refresh token.
/// IpAddress and UserAgent are forwarded from the HTTP context for audit.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password,
    string? IpAddress,
    string? UserAgent) : ICommand<Result<AuthResponse>>;
