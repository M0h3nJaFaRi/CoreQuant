using System.Collections.Concurrent;
using Common.Application.Caching;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// Process-local request cache for unit tests only. Production must use Redis via Infrastructure.
/// </summary>
internal sealed class InMemoryRequestCache : IRequestCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<(bool Hit, T? Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_entries.TryGetValue(key, out var entry) && entry.ExpiresAtUtc > DateTimeOffset.UtcNow)
        {
            return Task.FromResult((true, (T?)entry.Value));
        }

        return Task.FromResult<(bool, T?)>((false, default));
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _entries[key] = new CacheEntry(value!, DateTimeOffset.UtcNow.Add(duration));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_entries.TryRemove(key, out _));
    }

    private sealed record CacheEntry(object Value, DateTimeOffset ExpiresAtUtc);
}
