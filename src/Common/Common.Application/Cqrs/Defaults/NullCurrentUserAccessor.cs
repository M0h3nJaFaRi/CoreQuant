using Common.Application.Authorization.Abstraction;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// No authenticated user (hosts replace with ACL-backed accessor).
/// </summary>
internal sealed class NullCurrentUserAccessor : ICurrentUserAccessor
{
    /// <inheritdoc />
    public Guid? UserId => null;
}