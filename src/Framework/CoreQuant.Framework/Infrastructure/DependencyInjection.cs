using Framework.Time;
using Framework.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;
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
}
