namespace Common.Application.Authorization.Abstraction;

/// <summary>
/// Provides the authenticated caller identity for application and infrastructure code.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// Snowflake user id when authenticated; otherwise <see langword="null"/>.
    /// </summary>
    Guid? UserId { get; }
}