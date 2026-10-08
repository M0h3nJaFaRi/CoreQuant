using Framework.Time;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using Framework.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Framework.Infrastructure.Configuration.Options;
using Framework.Infrastructure.Configuration.Validators;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Framework.Infrastructure;

/// <summary>
/// Registers Framework.Infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Framework.Infrastructure defaults (<see cref="ISystemClock"/>).
    /// </summary>
    public static IServiceCollection AddFrameworkInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<ISystemClock, SystemClock>();
        return services;
    }

    /// <summary>
    /// Registers a singleton <see cref="IConnectionMultiplexer"/> from <see cref="FrameworkRedisOptions"/>.
    /// </summary>
    public static IServiceCollection AddFrameworkRedis(
        this IServiceCollection services,
        Action<FrameworkRedisOptions>? configure = null)
        => AddFrameworkRedis(services, configuration: null, configure);

    /// <summary>
    /// Registers Redis multiplexer, binding <c>Infrastructure:Redis</c> when configuration is provided.
    /// </summary>
    public static IServiceCollection AddFrameworkRedis(
        this IServiceCollection services,
        IConfiguration? configuration,
        Action<FrameworkRedisOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<FrameworkRedisOptions>()
            .ValidateOnStart();
        services.TryAddSingleton<IValidateOptions<FrameworkRedisOptions>, FrameworkRedisOptionsValidator>();

        if (configuration is not null)
            optionsBuilder.Bind(configuration.GetSection(FrameworkRedisOptions.SectionName));

        if (configure is not null)
            optionsBuilder.Configure(configure);

        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptionsMonitor<FrameworkRedisOptions>>().CurrentValue;
            return ConnectionMultiplexer.Connect(options.ToConfigurationOptions());
        });

        return services;
    }
}
