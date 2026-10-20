using Common.Application.Authorization.Contracts;

namespace Common.Application.Authorization.Abstraction;

/// <summary>
/// Custom, type-level authorization policy for a specific request, used when a simple
/// permission list isn't enough (e.g. combining role + permission + feature flag).
/// Still type-level only — no access to resource IDs for resource-level checks.
/// </summary>
public interface IAuthorizationPolicy<TRequest>
{
    Task<AuthorizationResult> AuthorizeAsync(TRequest request, CancellationToken cancellationToken);
}
