using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.BuildingBlocks.Domain.Time;
using Logistics.Api.Identity.Application.Abstractions;
using Logistics.Api.Identity.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Identity.Application.Commands.Logout;

public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IClock _clock;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IClock clock,
        ILogger<LogoutCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtService.HashToken(command.RefreshToken);

        var token = await _userRepository.GetActiveRefreshTokenByHashAsync(tokenHash, cancellationToken);

        if (token is null)
        {
            // Idempotent: already revoked or never existed — treat as success
            return Result.Success();
        }

        token.Revoke("Logout", _clock);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token revoked for user {UserId}", token.UserId);

        return Result.Success();
    }
}
