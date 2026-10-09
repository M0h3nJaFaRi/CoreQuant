namespace Common.Application.Locking;

/// <summary>
/// An acquired Redis distributed lock that must be released or disposed.
/// </summary>
public interface IRedisDistributedLockHandle : IAsyncDisposable
{
    /// <summary>
    /// Locked resource name (logical, without Redis prefix).
    /// </summary>
    string Resource { get; }

    /// <summary>
    /// Releases the lock when still owned by this handle.
    /// </summary>
    Task ReleaseAsync(CancellationToken cancellationToken = default);
}
