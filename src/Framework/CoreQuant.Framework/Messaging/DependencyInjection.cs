using Framework.Messaging.Lease;
using Framework.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Framework.Messaging;

/// <summary>
/// Registers Framework.Messaging services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds messaging defaults. Uses <see cref="RedisWorkLease"/> when <see cref="IConnectionMultiplexer"/>
    /// is registered; otherwise <see cref="InMemoryWorkLease"/>.
    /// </summary>
    public static IServiceCollection AddFrameworkMessaging(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IWorkLease>(sp =>
        {
            var clock = sp.GetRequiredService<ISystemClock>();
            var multiplexer = sp.GetService<IConnectionMultiplexer>();
            return multiplexer is null
                ? new InMemoryWorkLease(clock)
                : new RedisWorkLease(multiplexer, clock);
        });

        return services;
    }
}
