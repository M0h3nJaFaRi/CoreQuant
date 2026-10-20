using Common.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreQuant.Application;

/// <summary>
/// Registers CoreQuant application services and the shared CQRS pipeline.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers CoreQuant command/query handlers and calls <see cref="Common.Application.DependencyInjection.AddCommonApplication"/>.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommonApplication(typeof(AssemblyMarker).Assembly);

        return services;
    }
}