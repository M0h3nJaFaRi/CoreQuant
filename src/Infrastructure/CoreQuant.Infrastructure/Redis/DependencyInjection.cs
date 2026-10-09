using Common.Application.Caching;
using Common.Application.Events;
using Common.Application.Idempotency;
using Common.Application.Locking;
using Common.Application.Queuing;
using Common.Application.RateLimiting;
using Common.Application.Tokens;
using CoreQuant.Infrastructure.Redis.Adapters.Caching;
using CoreQuant.Infrastructure.Redis.Adapters.Events;
using CoreQuant.Infrastructure.Redis.Adapters.Idempotency;
using CoreQuant.Infrastructure.Redis.Adapters.Locking;
using CoreQuant.Infrastructure.Redis.Adapters.Queuing;
using CoreQuant.Infrastructure.Redis.Adapters.RateLimiting;
using CoreQuant.Infrastructure.Redis.Adapters.Tokens;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.Configuration.Validators;
using CoreQuant.Infrastructure.Redis.Core;
using CoreQuant.Infrastructure.Redis.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CoreQuant.Infrastructure.Redis;

/// <summary>
/// Internal Redis registration for CoreQuant infrastructure.
/// </summary>
internal static class RedisServiceCollectionExtensions
{
    /// <summary>
    /// Registers Redis core and distributed CQRS adapters.
    /// </summary>
    internal static IServiceCollection AddCoreQuantRedis(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton<IValidateOptions<CoreQuantRedisOptions>, CoreQuantRedisOptionsValidator>();

        services.AddOptions<CoreQuantRedisOptions>().Bind(configuration.GetSection(CoreQuantRedisOptions.SectionName)).ValidateOnStart();

        services.AddSingleton<RedisCore>();
        services.AddSingleton<RedisMetrics>();
        services.AddSingleton<IRedisCore, RedisInstrumentedCore>();
        services.AddSingleton<IRequestCache, RedisRequestCache>();
        services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();
        services.AddSingleton<IRedisRateLimiter, RedisRateLimiter>();
        services.AddSingleton<IRedisTokenStore, RedisTokenStore>();
        services.AddSingleton<IRedisDistributedLock, RedisDistributedLock>();
        services.AddSingleton<IRedisEventStore, RedisEventStore>();
        services.AddSingleton<IRedisQueue, RedisQueue>();
        services.TryAddSingleton<RedisEventStoreOutboxPublisher>();

        services.AddHealthChecks().AddCheck<RedisHealthCheck>("redis", failureStatus: HealthStatus.Unhealthy, tags: ["ready"]);

        return services;
    }
}
