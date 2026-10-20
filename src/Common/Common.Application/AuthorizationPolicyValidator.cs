using Common.Application.Authorization.Abstraction;
using Common.Application.Authorization.Attributes;
using Common.Application.Cqrs.Abstractions.Commands;
using Common.Application.Cqrs.Abstractions.Queries;
using System.Reflection;

namespace Common.Application;

/// <summary>
/// Scans an assembly for CQRS request types and verifies each one has an explicit
/// authorization decision source: <see cref="IAllowAnonymous"/>,
/// <see cref="RequiresPermissionAttribute"/>, or a registered
/// <see cref="IAuthorizationPolicy{TRequest}"/>. Call this at startup so a missing
/// annotation fails the boot, not a production request.
/// </summary>
public static class AuthorizationPolicyValidator
{
    public static void ValidateAuthorizationPolicy(IServiceProvider serviceProvider, Assembly assembly)
    {
        var requestTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i => i == typeof(ICommand) ||
                        (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)) ||
                        (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>))))
            .ToArray();

        var missing = new List<string>();

        foreach (var type in requestTypes)
        {
            if (typeof(IAllowAnonymous).IsAssignableFrom(type))
                continue;

            var hasAttribute = type.GetCustomAttributes(typeof(RequiresPermissionAttribute), inherit: false).Length > 0;

            if (hasAttribute)
                continue;

            var policyType = typeof(IAuthorizationPolicy<>).MakeGenericType(type);
            var hasPolicy = serviceProvider.GetService(policyType) is not null;

            if (hasPolicy)
                continue;

            missing.Add(type.FullName ?? type.Name);
        }

        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"The following CQRS requests have no authorization decision (missing [RequiresPermission], " +
                $"IAllowAnonymous, or a registered IAuthorizationPolicy<T>): {string.Join(", ", missing)}");
    }
}