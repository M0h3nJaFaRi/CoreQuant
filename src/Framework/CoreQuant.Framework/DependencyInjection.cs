using Common.Framework.HealthChecks;
using Common.Presentation.Contract;
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
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Framework;

/// <summary>
/// Composite registration entry point for CoreQuant.Framework.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Framework services. Hosts must call this once; external layers must not register Framework modules directly.
    /// </summary>
    public static IServiceCollection AddFramework(this IServiceCollection services, IConfiguration configuration, params Assembly[] validationAssemblies)
    {
        services.RegisterInfrastructure(configuration)
                .RegisterObservability(configuration)
                .RegisterWebObservability(configuration)
                .RegisterLogging(configuration)
                .RegisterSecurity(configuration)
                .RegisterSerialization(configuration)
                .RegisterValidation(configuration, validationAssemblies);

        return services;
    }
    public static IHealthChecksBuilder AddServiceHealth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var identity = sp.GetRequiredService<ServiceIdentity>();
            return new ServiceHealthInfo(identity.Component);
        });

        return services.AddHealthChecks()
                       .AddCheck("self", () => HealthCheckResult.Healthy(), tags: [HealthTags.Live]);
    }

    private static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<ISystemClock, SystemClock>();

        return services;
    }
    private static IServiceCollection RegisterObservability(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(sp =>
        {
            var serviceIdentity = sp.GetRequiredService<ServiceIdentity>();

            var tag = new List<KeyValuePair<string, object?>>()
            {
                new("namespcae", serviceIdentity.Namespace),
                new("name", serviceIdentity.Name),
                new("component", serviceIdentity.Component),
                new("version", serviceIdentity.Version)
            };

            return new ActivitySource(name: serviceIdentity.Name, version: serviceIdentity.Version, tags: [.. tag]);
        });

        services.TryAddSingleton(sp =>
        {
            var serviceIdentity = sp.GetRequiredService<ServiceIdentity>();

            var tag = new List<KeyValuePair<string, object?>>()
            {
                new("namespcae", serviceIdentity.Namespace),
                new("name", serviceIdentity.Name),
                new("component", serviceIdentity.Component),
                new("version", serviceIdentity.Version)
            };

            return new Meter(name: serviceIdentity.Name, version: serviceIdentity.Version, tags: [.. tag]);
        });

        return services;
    }
    private static IServiceCollection RegisterWebObservability(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<FrameworkHealthOptions>().Bind(configuration.GetSection(FrameworkHealthOptions.SectionName));

        services.AddOptions<FrameworkMetricsOptions>().Bind(configuration.GetSection(FrameworkMetricsOptions.SectionName));

        return services;
    }
    private static IServiceCollection RegisterLogging(this IServiceCollection services, IConfiguration configuration)
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
            var identity = serviceProvider.GetRequiredService<ServiceIdentity>();

            SerilogConfigurationBuilder.Configure(loggerConfiguration, serilogOptions, syslogOptions, identity);
        });

        return services;
    }
    private static IServiceCollection RegisterSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EncryptionOptions>()
                .Bind(configuration.GetSection(EncryptionOptions.SectionName))
                .ValidateOnStart();

        services.TryAddSingleton<IValidateOptions<EncryptionOptions>, EncryptionOptionsValidator>();

        services.TryAddSingleton<IHashService, HashService>();
        services.TryAddSingleton<IAesEncryptionService, AesEncryptionService>();
        services.TryAddSingleton<IRsaEncryptionService, RsaEncryptionService>();

        return services;
    }
    private static IServiceCollection RegisterSerialization(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IJsonSerializer, SystemTextJsonSerializer>();

        return services;
    }
    private static IServiceCollection RegisterValidation(this IServiceCollection services, IConfiguration configuration, Assembly[] validationAssemblies)
    {
        if (validationAssemblies is not { Length: > 0 })
            return services;

        services.AddValidatorsFromAssemblies(validationAssemblies, includeInternalTypes: true);

        return services;
    }
}