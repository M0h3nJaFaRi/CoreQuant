using Framework.Messaging.Lease;
using Framework.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Framework.Messaging;

/// <summary>
/// Registers Framework.Messaging services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds messaging defaults using <see cref="InMemoryWorkLease"/>.
    /// Product infrastructure may replace <see cref="IWorkLease"/> with a distributed implementation.
    /// </summary>
    public static IServiceCollection AddFrameworkMessaging(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IWorkLease>(sp =>
            new InMemoryWorkLease(sp.GetRequiredService<ISystemClock>()));

        return services;
    }
}
