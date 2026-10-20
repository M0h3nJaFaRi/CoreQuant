using Common.Application.Authorization.Contracts;

namespace Common.Application.Authorization.Abstraction;

/// <summary>
/// Authorizes CQRS use cases for the current caller.
/// </summary>
public interface IAuthorizationService
{
    Task<AuthorizationResult> AuthorizeAsync<TRequest>(TRequest request, CancellationToken cancellationToken);
}