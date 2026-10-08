using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure;

/// <summary>
/// Registers shared Common infrastructure adapters.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Reserved for future shared non-product infrastructure adapters.
    /// Redis is registered by <c>CoreQuant.Infrastructure.AddCoreQuantInfrastructure</c>.
    /// </summary>
    public static IServiceCollection AddCommonInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}
