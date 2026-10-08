using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Validation;

/// <summary>
/// Registers FluentValidation validators from assemblies. Does not own domain validators or CQRS pipeline behavior.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds FluentValidation and scans <paramref name="assemblies"/> for validators.
    /// </summary>
    public static IServiceCollection AddFrameworkValidation(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (assemblies is null || assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly is required.", nameof(assemblies));
        }

        services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);
        return services;
    }
}
