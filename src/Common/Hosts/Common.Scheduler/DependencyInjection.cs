using System.Reflection;
using Common.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Scheduler;

/// <summary>
/// Shared composition for Scheduler host processes (triggers Worker jobs; no Outbox BackgroundService).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers observability, security, CQRS buses/behaviors, and FluentValidation for the given application assemblies.
    /// Job triggers are registered by the service host.
    /// </summary>
    /// <param name="serviceName">Fallback service name when <c>ApplicationName</c> is not configured.</param>
    public static IServiceCollection AddCommonScheduler(
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
