using Common.Application.Authorization;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// No authenticated user (hosts replace with ACL-backed accessor).
/// </summary>
internal sealed class NullCurrentUserAccessor : ICurrentUserAccessor
{
    /// <inheritdoc />
    public long? UserId => null;
}
