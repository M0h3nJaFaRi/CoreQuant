using Framework.Time;
using System.Collections.Concurrent;

namespace Framework.Messaging.Lease;

/// <summary>
/// Process-local <see cref="IWorkLease"/> suitable for single-instance hosts and tests.
/// </summary>
internal sealed class InMemoryWorkLease(ISystemClock clock) : IWorkLease
{
    private readonly ConcurrentDictionary<string, LeaseEntry> _leases = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<IWorkLeaseHandle?> TryAcquireAsync(
        string resourceKey,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKey);
        if (leaseDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(leaseDuration), leaseDuration, "Lease duration must be positive.");

        cancellationToken.ThrowIfCancellationRequested();

        var now = clock.UtcNow;
        var token = Guid.NewGuid().ToString("N");
        var expiresAt = now.Add(leaseDuration);

        while (true)
        {
            if (_leases.TryGetValue(resourceKey, out var existing))
            {
                if (existing.ExpiresAtUtc > now)
                    return Task.FromResult<IWorkLeaseHandle?>(null);

                var replacement = new LeaseEntry(token, expiresAt);
                if (_leases.TryUpdate(resourceKey, replacement, existing))
                    return Task.FromResult<IWorkLeaseHandle?>(new Handle(this, resourceKey, token, expiresAt));

                continue;
            }

            if (_leases.TryAdd(resourceKey, new LeaseEntry(token, expiresAt)))
                return Task.FromResult<IWorkLeaseHandle?>(new Handle(this, resourceKey, token, expiresAt));
        }
    }

    private void Release(string resourceKey, string token)
    {
        if (_leases.TryGetValue(resourceKey, out var existing) && existing.Token == token)
            _leases.TryRemove(new KeyValuePair<string, LeaseEntry>(resourceKey, existing));
    }

    private sealed record LeaseEntry(string Token, DateTimeOffset ExpiresAtUtc);

    private sealed class Handle(InMemoryWorkLease owner, string resourceKey, string token, DateTimeOffset expiresAtUtc) : IWorkLeaseHandle
    {
        private int _disposed;

        public string ResourceKey { get; } = resourceKey;

        public DateTimeOffset ExpiresAtUtc { get; } = expiresAtUtc;

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                owner.Release(ResourceKey, token);

            return ValueTask.CompletedTask;
        }
    }
}
