namespace Common.Application.Authorization;

/// <summary>
/// Application-level authorization for CQRS use cases (not ASP.NET authentication).
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Returns true when the current caller may execute <paramref name="requestType"/>.
    /// </summary>
    Task<bool> AuthorizeAsync(Type requestType, CancellationToken cancellationToken = default);
}
