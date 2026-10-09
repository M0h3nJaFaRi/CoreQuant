using System.Reflection;
using Common.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Consumer;

/// <summary>
/// Shared composition for Consumer host processes.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers CQRS buses/behaviors for the given application assemblies.
    /// </summary>
    public static IServiceCollection AddCommonConsumer(
        this IServiceCollection services,
        string serviceName,
        IConfiguration configuration,
        params Assembly[] applicationAssemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddCommonApplication(applicationAssemblies);

        return services;
    }
}
