using Common.Application.Authorization.Abstraction;
using Common.Application.Authorization.Attributes;
using Common.Application.Authorization.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Common.Application.Authorization;

/// <summary>
/// Default authorization service: resolves a request's required permissions from
/// <see cref="RequiresPermissionAttribute"/> (cached via reflection per type) or a
/// registered <see cref="IAuthorizationPolicy{TRequest}"/>, and checks them against
/// the caller's effective permissions. Fails closed when no decision source exists —
/// the startup validator should prevent this from ever happening in practice.
/// </summary>
internal sealed class DefaultAuthorizationService(ICurrentUserAccessor currentUser, 
                                                  IServiceProvider serviceProvider,
                                                  IPermissionProvider permissionProvider) : IAuthorizationService
{
    private static readonly ConcurrentDictionary<Type, RequiresPermissionAttribute?> AttributeCache = new();

    public async Task<AuthorizationResult> AuthorizeAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        if (request is IAllowAnonymous) 
            return AuthorizationResult.Allow();

        // Custom policy takes precedence if one is registered for this request type.
        var policy = serviceProvider.GetService<IAuthorizationPolicy<TRequest>>();
        if (policy is not null)
            return await policy.AuthorizeAsync(request, cancellationToken).ConfigureAwait(false);

        var attribute = AttributeCache.GetOrAdd(typeof(TRequest),
            t => t.GetCustomAttributes(typeof(RequiresPermissionAttribute), inherit: false)
                  .Cast<RequiresPermissionAttribute>()
                  .SingleOrDefault());

        if (attribute is null)
            return AuthorizationResult.Deny("no_policy_configured"); // fail closed — should never happen if startup validation ran

        var userId = currentUser.UserId;
        if (userId is null)
            return AuthorizationResult.Deny("unauthenticated");

        var effective = await permissionProvider.GetEffectivePermissionsAsync(userId.Value, cancellationToken).ConfigureAwait(false);

        var satisfied = attribute.RequireAny
            ? attribute.Permissions.Any(effective.Contains)
            : attribute.Permissions.All(effective.Contains);

        return satisfied ? AuthorizationResult.Allow() : AuthorizationResult.Deny("missing_permission");
    }
}