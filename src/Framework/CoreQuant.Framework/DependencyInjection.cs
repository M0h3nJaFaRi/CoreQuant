using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using FluentValidation;
using Framework.Infrastructure.Time;
using Framework.Observability.Configuration.Options;
using Framework.Observability.Configuration.Validators;
using Framework.Observability.Logging;
using Framework.Security.Configuration.Options;
using Framework.Security.Configuration.Validators;
using Framework.Security.Encryption;
using Framework.Security.Hashing;
using Framework.Serialization.Json;
using Framework.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;

namespace Framework;

/// <summary>
/// Composite registration entry point for CoreQuant.Framework.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Framework services. Hosts must call this once; external layers must not register Framework modules directly.
    /// </summary>
    public static IServiceCollection AddFramework(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName,
        params Assembly[] validationAssemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);

        RegisterInfrastructure(services);
        RegisterObservability(services, applicationName);
        RegisterWebObservability(services, configuration);
        RegisterLogging(services, configuration);
        RegisterSecurity(services, configuration);
        RegisterSerialization(services);
        RegisterValidation(services, validationAssemblies);

        return services;
    }

    private static void RegisterInfrastructure(IServiceCollection services)
    {
        services.TryAddSingleton<ISystemClock, SystemClock>();
    }

    private static void RegisterObservability(IServiceCollection services, string applicationName)
    {
        services.AddOptions<FrameworkObservabilityOptions>()
            .Configure(options => options.ServiceName = applicationName)
            .ValidateOnStart();
        services.TryAddSingleton<IValidateOptions<FrameworkObservabilityOptions>, FrameworkObservabilityOptionsValidator>();

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
    }

    private static void RegisterWebObservability(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<FrameworkHealthOptions>()
            .Bind(configuration.GetSection(FrameworkHealthOptions.SectionName));

        services.AddOptions<FrameworkMetricsOptions>()
            .Bind(configuration.GetSection(FrameworkMetricsOptions.SectionName));
    }

    private static void RegisterLogging(IServiceCollection services, IConfiguration configuration)
    {
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

            SerilogConfigurationBuilder.Configure(
                loggerConfiguration,
                serilogOptions,
                syslogOptions,
                observability.ServiceName);
        });
    }

    private static void RegisterSecurity(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EncryptionOptions>()
            .Bind(configuration.GetSection(EncryptionOptions.SectionName))
            .ValidateOnStart();
        services.TryAddSingleton<IValidateOptions<EncryptionOptions>, EncryptionOptionsValidator>();

        services.TryAddSingleton<IHashService, HashService>();
        services.TryAddSingleton<IAesEncryptionService, AesEncryptionService>();
        services.TryAddSingleton<IRsaEncryptionService, RsaEncryptionService>();
    }

    private static void RegisterSerialization(IServiceCollection services)
    {
        services.TryAddSingleton<IJsonSerializer, SystemTextJsonSerializer>();
    }

    private static void RegisterValidation(IServiceCollection services, Assembly[] validationAssemblies)
    {
        if (validationAssemblies is not { Length: > 0 })
        {
            return;
        }

        services.AddValidatorsFromAssemblies(validationAssemblies, includeInternalTypes: true);
    }
}
