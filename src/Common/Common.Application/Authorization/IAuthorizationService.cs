namespace Common.Application.Authorization;

/// <summary>
/// Authorizes CQRS use cases for the current caller.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Returns whether the current caller may execute the given request type.
    /// </summary>
    Task<bool> AuthorizeAsync(Type requestType, CancellationToken cancellationToken = default);
}
