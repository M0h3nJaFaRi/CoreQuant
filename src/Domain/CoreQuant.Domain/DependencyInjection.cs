using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreQuant.Domain;

/// <summary>
/// Domain-layer composition hook (reserved for domain services that require DI registration).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers domain-layer dependencies. Currently a no-op; extend when domain services need DI.
    /// </summary>
    public static IServiceCollection AddDomain(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
