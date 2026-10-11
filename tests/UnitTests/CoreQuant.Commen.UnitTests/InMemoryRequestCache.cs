using System.Collections.Concurrent;
using Common.Application.Caching;

namespace CoreQuant.Commen.UnitTests;

/// <summary>
/// In-process <see cref="IRequestCache"/> for unit tests.
/// </summary>
internal sealed class InMemoryRequestCache : IRequestCache
{
    private readonly ConcurrentDictionary<string, object> _entries = new();

    /// <inheritdoc />
    public Task<(bool Hit, T? Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_entries.TryGetValue(key, out var value) && value is T typed)
        {
            return Task.FromResult<(bool, T?)>((true, typed));
        }

        return Task.FromResult<(bool, T?)>((false, default));
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(value);
        _ = duration;
        _entries[key] = value!;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_entries.TryRemove(key, out _));
    }
}
