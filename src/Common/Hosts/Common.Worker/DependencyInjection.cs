using System.Reflection;
using Common.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Worker;

/// <summary>
/// Shared composition for Worker host processes (no HTTP pipeline).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers observability, security, CQRS buses/behaviors, and FluentValidation for the given application assemblies.
    /// Persistence and jobs are registered by the service host.
    /// </summary>
    /// <param name="serviceName">Fallback service name when <c>ApplicationName</c> is not configured.</param>
    public static IServiceCollection AddCommonWorker(
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
