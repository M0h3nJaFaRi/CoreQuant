using Common.Application.Authorization;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// Default authorization that allows every use case (replace in hosts with real policies).
/// </summary>
internal sealed class AllowAllAuthorizationService : IAuthorizationService
{
    /// <inheritdoc />
    public Task<bool> AuthorizeAsync(Type requestType, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}
