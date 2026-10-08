using Common.Application.Caching;
using Common.Application.Idempotency;
using Common.Application.RateLimiting;
using CoreQuant.Infrastructure.Redis.Caching;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.Configuration.Validators;
using CoreQuant.Infrastructure.Redis.Idempotency;
using CoreQuant.Infrastructure.Redis.Messaging;
using CoreQuant.Infrastructure.Redis.RateLimiting;
using Framework.Messaging.Lease;
using Framework.Serialization;
using Framework.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CoreQuant.Infrastructure.Redis;

/// <summary>
/// Internal Redis registration for CoreQuant infrastructure.
/// </summary>
internal static class RedisServiceCollectionExtensions
{
    /// <summary>
    /// Registers Redis multiplexer and distributed adapters.
    /// </summary>
    internal static IServiceCollection AddCoreQuantRedis(
        IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var redisOptionsBuilder = services.AddOptions<CoreQuantRedisOptions>()
            .ValidateOnStart();
        services.TryAddSingleton<IValidateOptions<CoreQuantRedisOptions>, CoreQuantRedisOptionsValidator>();
        redisOptionsBuilder.Bind(configuration.GetSection(CoreQuantRedisOptions.SectionName));

        var namespaceOptionsBuilder = services.AddOptions<RedisKeyNamespaceOptions>()
            .ValidateOnStart();
        services.TryAddSingleton<IValidateOptions<RedisKeyNamespaceOptions>, RedisKeyNamespaceOptionsValidator>();
        namespaceOptionsBuilder.Configure<CoreQuantRedisOptions>((namespaceOptions, redisOptions) =>
        {
            namespaceOptions.Service = serviceName.Trim();
            namespaceOptions.Host = string.IsNullOrWhiteSpace(redisOptions.HostIdentity)
                ? Environment.MachineName
                : redisOptions.HostIdentity.Trim();
        });

        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptionsMonitor<CoreQuantRedisOptions>>().CurrentValue;
            return ConnectionMultiplexer.Connect(options.ToConfigurationOptions());
        });

        services.AddFrameworkSerialization();

        services.RemoveAll<IRequestCache>();
        services.RemoveAll<IIdempotencyStore>();
        services.RemoveAll<IRateLimiter>();
        services.RemoveAll<IWorkLease>();

        services.AddSingleton<IRequestCache, RedisRequestCache>();
        services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();
        services.AddSingleton<IRateLimiter, RedisRateLimiter>();
        services.AddSingleton<IWorkLease, RedisWorkLease>();

        return services;
    }
}
