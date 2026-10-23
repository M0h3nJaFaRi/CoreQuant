using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Common.Framework.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Framework.HealthChecks;
public static class ServiceHealthEndpoints
{
    public static IEndpointRouteBuilder MapServiceHealth(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains(HealthTags.Live),
            ResponseWriter = HealthResponseWriter.Minimal
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains(HealthTags.Ready),
            ResponseWriter = HealthResponseWriter.Minimal
        });

        endpoints.MapHealthChecks("/health/startup", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains(HealthTags.Startup),
            ResponseWriter = HealthResponseWriter.Minimal
        });

        endpoints.MapHealthChecks("/health/details", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains(HealthTags.Detail),
            ResponseWriter = HealthResponseWriter.Detailed
        });

        return endpoints;
    }
}