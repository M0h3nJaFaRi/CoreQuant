using Framework.Observability.Configuration.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Framework.Web;

/// <summary>
/// Web host health and metrics endpoint registration.
/// </summary>
public static class WebObservabilityExtensions
{
    /// <summary>
    /// Binds <see cref="FrameworkHealthOptions"/> and <see cref="FrameworkMetricsOptions"/> from configuration.
    /// </summary>
    public static IServiceCollection AddFrameworkWebObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);

        services.AddOptions<FrameworkHealthOptions>()
            .Bind(configuration.GetSection(FrameworkHealthOptions.SectionName));

        services.AddOptions<FrameworkMetricsOptions>()
            .Bind(configuration.GetSection(FrameworkMetricsOptions.SectionName));

        return services;
    }

    /// <summary>
    /// Maps liveness/readiness health checks and the optional metrics endpoint.
    /// </summary>
    public static WebApplication MapFrameworkWebObservabilityEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var health = app.Services.GetRequiredService<IOptions<FrameworkHealthOptions>>().Value;
        var metrics = app.Services.GetRequiredService<IOptions<FrameworkMetricsOptions>>().Value;

        app.MapHealthChecks(
            health.LivenessPath,
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live"),
            });

        app.MapHealthChecks(
            health.ReadinessPath,
            new HealthCheckOptions
            {
                Predicate = _ => true,
            });

        if (metrics.Enabled && !string.IsNullOrWhiteSpace(metrics.Path))
        {
            app.MapGet(
                metrics.Path.TrimEnd('/'),
                async context =>
                {
                    context.Response.ContentType = "text/plain; version=0.0.4";
                    await context.Response.WriteAsync("# metrics disabled until exporter is configured\n").ConfigureAwait(false);
                }).ExcludeFromDescription();
        }

        return app;
    }
}
