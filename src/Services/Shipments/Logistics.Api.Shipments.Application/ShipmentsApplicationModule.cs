using FluentValidation;
using Logistics.Api.Shipments.Application.Abstractions;
using Logistics.Api.Shipments.Application.Commands.CreateShipment;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Api.Shipments.Application;

public static class ShipmentsApplicationModule
{
    public static IServiceCollection AddShipmentsApplicationServices(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ShipmentsApplicationModule).Assembly));

        services.AddValidatorsFromAssemblyContaining<CreateShipmentCommandValidator>();

        return services;
    }
}
