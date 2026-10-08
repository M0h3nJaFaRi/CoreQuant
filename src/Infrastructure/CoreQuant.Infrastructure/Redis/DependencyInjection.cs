using Common.Application.Caching;
using Common.Application.Idempotency;
using Common.Application.RateLimiting;
using CoreQuant.Infrastructure.Redis.Adapters.Caching;
using CoreQuant.Infrastructure.Redis.Adapters.Idempotency;
using CoreQuant.Infrastructure.Redis.Adapters.RateLimiting;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.Configuration.Validators;
using CoreQuant.Infrastructure.Redis.Core;
using Framework.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    internal static IServiceCollection AddCoreQuantRedis(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton<IValidateOptions<CoreQuantRedisOptions>, CoreQuantRedisOptionsValidator>();
        
        services.AddOptions<CoreQuantRedisOptions>().Bind(configuration.GetSection(CoreQuantRedisOptions.SectionName)).ValidateOnStart();

        services.AddFrameworkSerialization();
        services.AddSingleton<IRedisCore, RedisCore>();

        services.RemoveAll<IRequestCache>();
        services.RemoveAll<IIdempotencyStore>();
        services.RemoveAll<IRateLimiter>();

        services.AddSingleton<IRequestCache, RedisRequestCache>();
        services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();
        services.AddSingleton<IRateLimiter, RedisRateLimiter>();

        return services;
    }
}
