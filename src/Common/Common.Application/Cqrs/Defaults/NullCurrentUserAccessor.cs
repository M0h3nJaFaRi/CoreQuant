using Common.Application.Authorization;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// Default current-user accessor with no authenticated user.
/// </summary>
internal sealed class NullCurrentUserAccessor : ICurrentUserAccessor
{
    /// <inheritdoc />
    public long? UserId => null;
}
