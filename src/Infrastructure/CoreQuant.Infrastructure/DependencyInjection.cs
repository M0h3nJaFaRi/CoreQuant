using CoreQuant.Infrastructure.Redis;
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
    public static IServiceCollection AddCoreQuantInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        RedisServiceCollectionExtensions.AddCoreQuantRedis(services, configuration);

        return services;
    }
}
