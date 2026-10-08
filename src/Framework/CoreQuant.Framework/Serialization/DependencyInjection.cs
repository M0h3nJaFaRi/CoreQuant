using Framework.Serialization.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Framework.Serialization;

/// <summary>
/// Registers Framework.Serialization services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the default <see cref="IJsonSerializer"/> implementation.
    /// </summary>
    public static IServiceCollection AddFrameworkSerialization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IJsonSerializer, SystemTextJsonSerializer>();
        return services;
    }
}
