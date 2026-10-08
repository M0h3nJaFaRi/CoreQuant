using Common.Application.Caching;
using Common.Application.Idempotency;
using Common.Infrastructure.Caching;
using Common.Infrastructure.Configuration.Options;
using Common.Infrastructure.Configuration.Validators;
using Common.Infrastructure.Idempotency;
using Framework.Infrastructure;
using Framework.Infrastructure.Configuration.Options;
using Framework.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Common.Infrastructure;

/// <summary>
/// Registers Common infrastructure adapters (Redis cache, idempotency, …).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers distributed Redis implementations for CQRS cache and idempotency.
    /// Requires <c>AddFrameworkRedis</c> and serialization to already be registered, or use
    /// <see cref="AddCommonInfrastructureWithRedis"/>.
    /// </summary>
    public static IServiceCollection AddCommonInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<RedisRequestCacheOptions>()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<RedisRequestCacheOptions>, RedisRequestCacheOptionsValidator>();

        services.AddOptions<RedisIdempotencyStoreOptions>()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<RedisIdempotencyStoreOptions>, RedisIdempotencyStoreOptionsValidator>();

        services.AddFrameworkSerialization();
        services.TryAddSingleton<IRequestCache, RedisRequestCache>();
        services.TryAddSingleton<IIdempotencyStore, RedisIdempotencyStore>();

        return services;
    }

    /// <summary>
    /// Registers Redis multiplexer from <c>Infrastructure:Redis</c> plus Common infrastructure adapters.
    /// </summary>
    public static IServiceCollection AddCommonInfrastructureWithRedis(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<FrameworkRedisOptions>? configureRedis = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddFrameworkRedis(configuration, configureRedis);
        return services.AddCommonInfrastructure();
    }
}
