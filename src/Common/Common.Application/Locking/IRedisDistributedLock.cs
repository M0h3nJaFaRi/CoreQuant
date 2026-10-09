namespace Common.Application.Locking;

/// <summary>
/// Redis-backed distributed lock for cross-pod coordination.
/// </summary>
public interface IRedisDistributedLock
{
    /// <summary>
    /// Attempts to acquire an exclusive lock on <paramref name="resource"/> with mandatory TTL.
    /// </summary>
    Task<IRedisDistributedLockHandle?> TryAcquireAsync(
        string resource,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);
}
