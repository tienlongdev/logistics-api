using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.BuildingBlocks.Domain.Time;
using Logistics.Api.Identity.Application.Abstractions;
using Logistics.Api.Identity.Application.Dtos;
using Logistics.Api.Identity.Application.Errors;
using Logistics.Api.Identity.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Identity.Application.Commands.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IClock _clock;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IClock clock,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailWithRolesAsync(command.Email, cancellationToken);

        // Constant-time failure: same error for "not found" and "wrong password"
        if (user is null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for email {Email}", command.Email);
            return Result<AuthResponse>.Failure(IdentityErrors.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt on inactive account {UserId}", user.Id);
            return Result<AuthResponse>.Failure(IdentityErrors.UserInactive);
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, roles);

        var rawRefreshToken = _jwtService.GenerateRefreshTokenRaw();
        var tokenHash = _jwtService.HashToken(rawRefreshToken);
        var expiresAt = _clock.UtcNow.AddDays(_jwtService.RefreshTokenExpiresInDays);

        user.AddRefreshToken(tokenHash, expiresAt, command.IpAddress, command.UserAgent, _clock);

        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return Result<AuthResponse>.Success(
            new AuthResponse(accessToken, rawRefreshToken, _jwtService.AccessTokenExpiresInSeconds));
    }
}
