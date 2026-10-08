using System.Collections.Concurrent;
using Common.Application.Idempotency;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// Process-local idempotency store for unit tests only. Production must use Redis via Infrastructure.
/// </summary>
internal sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, StoreEntry> _entries = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<(bool Found, TResponse? Response)> TryGetAsync<TResponse>(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_entries.TryGetValue(key, out var entry) && entry.ExpiresAtUtc > DateTimeOffset.UtcNow)
        {
            return Task.FromResult((true, (TResponse?)entry.Response));
        }

        return Task.FromResult<(bool, TResponse?)>((false, default));
    }

    /// <inheritdoc />
    public Task SetAsync<TResponse>(
        string key,
        TResponse response,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _entries[key] = new StoreEntry(response!, DateTimeOffset.UtcNow.Add(ttl));
        return Task.CompletedTask;
    }

    private sealed record StoreEntry(object Response, DateTimeOffset ExpiresAtUtc);
}
