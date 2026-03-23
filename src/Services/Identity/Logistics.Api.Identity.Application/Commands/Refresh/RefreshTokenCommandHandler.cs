using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.BuildingBlocks.Domain.Time;
using Logistics.Api.Identity.Application.Abstractions;
using Logistics.Api.Identity.Application.Dtos;
using Logistics.Api.Identity.Application.Errors;
using Logistics.Api.Identity.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Identity.Application.Commands.Refresh;

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IClock _clock;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IClock clock,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtService.HashToken(command.RefreshToken);

        var existingToken = await _userRepository.GetActiveRefreshTokenByHashAsync(tokenHash, cancellationToken);
        if (existingToken is null)
        {
            _logger.LogWarning("Invalid or expired refresh token attempt");
            return Result<AuthResponse>.Failure(IdentityErrors.InvalidRefreshToken);
        }

        var user = await _userRepository.GetByIdWithRolesAsync(existingToken.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            _logger.LogWarning("Refresh token belongs to missing/inactive user {UserId}", existingToken.UserId);
            return Result<AuthResponse>.Failure(IdentityErrors.InvalidCredentials);
        }

        // Rotate: revoke old token
        existingToken.Revoke("Token rotation", _clock);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var newAccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, roles);

        var newRawToken = _jwtService.GenerateRefreshTokenRaw();
        var newHash = _jwtService.HashToken(newRawToken);
        var expiresAt = _clock.UtcNow.AddDays(_jwtService.RefreshTokenExpiresInDays);

        user.AddRefreshToken(newHash, expiresAt, command.IpAddress, command.UserAgent, _clock);

        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token rotated for user {UserId}", user.Id);

        return Result<AuthResponse>.Success(
            new AuthResponse(newAccessToken, newRawToken, _jwtService.AccessTokenExpiresInSeconds));
    }
}
