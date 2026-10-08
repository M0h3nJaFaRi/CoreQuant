namespace Common.Application.Authorization;

/// <summary>
/// Optional ambient user context for logging and authorization.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// Current user id when authenticated; otherwise null.
    /// </summary>
    long? UserId { get; }
}
