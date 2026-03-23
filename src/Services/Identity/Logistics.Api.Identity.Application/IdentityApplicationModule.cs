using FluentValidation;
using Logistics.Api.Identity.Application.Commands.Login;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Api.Identity.Application;

/// <summary>
/// Registers Application-layer services for the Identity module:
/// MediatR handlers and FluentValidation validators.
/// Called from the Infrastructure installer.
/// </summary>
public static class IdentityApplicationModule
{
    public static IServiceCollection AddIdentityApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(IdentityApplicationModule).Assembly));

        services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();

        return services;
    }
}
