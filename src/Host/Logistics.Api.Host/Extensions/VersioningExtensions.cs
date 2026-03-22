using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

namespace Logistics.Api.Host.Extensions;

public static class VersioningExtensions
{
    public static IServiceCollection AddApiVersioningWithExplorer(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;

                // URL segment: /api/v{version}/...
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV"; // v1, v1.0
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }
}
