namespace Framework.Messaging.Lease;

/// <summary>
/// A held work lease. Dispose to release.
/// </summary>
public interface IWorkLeaseHandle : IAsyncDisposable
{
    /// <summary>
    /// Resource key protected by this lease.
    /// </summary>
    string ResourceKey { get; }

    /// <summary>
    /// UTC expiry of the lease token.
    /// </summary>
    DateTimeOffset ExpiresAtUtc { get; }
}
