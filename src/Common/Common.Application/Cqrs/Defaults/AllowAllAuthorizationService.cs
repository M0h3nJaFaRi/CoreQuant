using Common.Application.Authorization;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// Permits all requests until a host registers a real authorization service.
/// </summary>
internal sealed class AllowAllAuthorizationService : IAuthorizationService
{
    /// <inheritdoc />
    public Task<bool> AuthorizeAsync(Type requestType, CancellationToken cancellationToken = default) =>
        Task.FromResult(true);
}
