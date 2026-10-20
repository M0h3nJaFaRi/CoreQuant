using Common.Application.Authorization.Abstraction;
using Common.Application.Authorization.Contracts;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// Permits all requests until a host registers a real authorization service.
/// </summary>
internal sealed class AllowAllAuthorizationService : IAuthorizationService
{
    /// <inheritdoc />
    public Task<AuthorizationResult> AuthorizeAsync<TRequest>(TRequest request, CancellationToken cancellationToken) => Task.FromResult(AuthorizationResult.Allow());
}