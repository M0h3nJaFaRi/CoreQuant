namespace Framework.Messaging.Lease;

/// <summary>
/// Exclusive work lease used to coordinate single-worker processing of a resource.
/// </summary>
public interface IWorkLease
{
    /// <summary>
    /// Attempts to acquire an exclusive lease for <paramref name="resourceKey"/>.
    /// Returns <c>null</c> when the lease is already held.
    /// </summary>
    Task<IWorkLeaseHandle?> TryAcquireAsync(
        string resourceKey,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);
}
