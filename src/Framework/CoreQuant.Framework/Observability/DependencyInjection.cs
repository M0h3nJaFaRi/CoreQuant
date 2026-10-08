using Serilog;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;
using Framework.Observability.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Framework.Observability.Configuration.Options;
using Framework.Observability.Configuration.Validators;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Framework.Observability;

/// <summary>
/// Registers Framework.Observability primitives (logging, health, instruments).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds health checks and shared <see cref="ActivitySource"/> / <see cref="Meter"/> for the service.
    /// </summary>
    public static IServiceCollection AddFrameworkObservability(
        this IServiceCollection services,
        Action<FrameworkObservabilityOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<FrameworkObservabilityOptions>()
            .ValidateOnStart();
        services.TryAddSingleton<IValidateOptions<FrameworkObservabilityOptions>, FrameworkObservabilityOptionsValidator>();

        if (configure is not null)
            optionsBuilder.Configure(configure);

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<FrameworkObservabilityOptions>>().Value;
            return new ActivitySource(options.ServiceName);
        });

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<FrameworkObservabilityOptions>>().Value;
            return new Meter(options.ServiceName);
        });

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

        return services;
    }

    /// <summary>
    /// Registers Serilog from <c>Framework:Serilog</c> and optional syslog from <c>Framework:Syslog</c>.
    /// Call after <see cref="AddFrameworkObservability"/> so <c>Application</c> enrich uses <see cref="FrameworkObservabilityOptions.ServiceName"/>.
    /// </summary>
    public static IServiceCollection AddFrameworkLogging(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<SerilogOptions>()
            .Bind(configuration.GetSection(SerilogOptions.SectionName))
            .ValidateOnStart();

        services.TryAddSingleton<IValidateOptions<SerilogOptions>, SerilogOptionsValidator>();

        services.AddOptions<SyslogOptions>()
            .Bind(configuration.GetSection(SyslogOptions.SectionName))
            .ValidateOnStart();

        services.TryAddSingleton<IValidateOptions<SyslogOptions>, SyslogOptionsValidator>();

        services.AddSerilog((serviceProvider, loggerConfiguration) =>
        {
            var serilogOptions = serviceProvider.GetRequiredService<IOptionsMonitor<SerilogOptions>>().CurrentValue;
            var syslogOptions = serviceProvider.GetRequiredService<IOptionsMonitor<SyslogOptions>>().CurrentValue;
            var observability = serviceProvider.GetRequiredService<IOptionsMonitor<FrameworkObservabilityOptions>>().CurrentValue;

            SerilogConfigurationBuilder.Configure(loggerConfiguration, serilogOptions, syslogOptions, observability.ServiceName);
        });

        return services;
    }
}
