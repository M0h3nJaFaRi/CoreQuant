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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCoreQuantRedis(configuration);

        return services;
    }
}
