namespace Common.Application.Authorization.Abstraction;

/// <summary>
/// Resolves the effective (role + direct) permission set for the current caller.
/// Implementations should cache aggressively (Redis) since this runs on every request.
/// </summary>
public interface IPermissionProvider
{
    Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken cancellationToken);
}