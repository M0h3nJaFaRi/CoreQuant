using Common.Application.Authorization;
using Common.Application.Cqrs.Abstractions;

namespace CoreQuant.Infrastructure.Acl.Authorization;

/// <summary>
/// Sample use-case policy: commands require an authenticated user, queries stay anonymous.
/// Replace with real per-use-case policies as the product domain grows.
/// </summary>
internal sealed class SampleAuthorizationService : IAuthorizationService
{
    private readonly ICurrentUserAccessor _currentUser;

    public SampleAuthorizationService(ICurrentUserAccessor currentUser)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public Task<bool> AuthorizeAsync(Type requestType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requestType);

        if (!IsCommand(requestType))
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(_currentUser.UserId is not null);
    }

    private static bool IsCommand(Type requestType)
    {
        if (typeof(ICommand).IsAssignableFrom(requestType))
        {
            return true;
        }

        return requestType.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }
}
