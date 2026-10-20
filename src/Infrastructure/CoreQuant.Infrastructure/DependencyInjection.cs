using Common.Framework.HealthChecks;
using CoreQuant.Infrastructure.Redis;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.Core;
using CoreQuant.Infrastructure.Redis.Observability;
using Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreQuant.Infrastructure;

/// <summary>
/// Registers CoreQuant infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers CoreQuant infrastructure including Redis-backed CQRS adapters.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCoreQuantRedis(configuration);

        return services;
    }
    public static IHealthChecksBuilder RegisterRedisHealthCheck(this IHealthChecksBuilder builder, IConfiguration configuration, string name = "redis")
    {
        var redis = configuration.GetSection(CoreQuantRedisOptions.SectionName).Get<CoreQuantRedisOptions>()
            ?? throw new InvalidOperationException($"Missing Redis configuration at {CoreQuantRedisOptions.SectionName}.");

        return builder.AddDependencyHealthCheck(name, redis.Health, HealthTags.Redis, sp => new RedisHealthCheck(sp.GetRequiredService<IRedisCore>(), redis.Health));
    }
}
