using System.Reflection;
using Framework.Security;
using Common.Application;
using Framework.Validation;
using Framework.Observability;
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

        var applicationName = ResolveApplicationName(configuration, serviceName);

        services.AddFrameworkObservability(options => options.ServiceName = applicationName);
        services.AddFrameworkLogging(configuration);
        services.AddFrameworkSecurity(configuration);
        services.AddCommonApplication(applicationAssemblies);

        if (applicationAssemblies is { Length: > 0 })
            services.AddFrameworkValidation(applicationAssemblies);

        return services;
    }

    private static string ResolveApplicationName(IConfiguration configuration, string fallback)
    {
        var configured = configuration["ApplicationName"];
        return string.IsNullOrWhiteSpace(configured) ? fallback : configured.Trim();
    }
}
