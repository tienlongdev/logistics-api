using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Logistics.Api.Host.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiWithVersioning(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "Logistics API",
                    Version = context.DocumentName,
                    Description = "Enterprise Logistics Backend (.NET 10) - Modular Monolith foundation"
                };
                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication MapOpenApiEndpoints(this WebApplication app)
    {
        // /openapi/v1.json, /openapi/v2.json...
        app.MapOpenApi("/openapi/{documentName}.json");

        // Scalar UI
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("Logistics API");
        });

        return app;
    }
}
