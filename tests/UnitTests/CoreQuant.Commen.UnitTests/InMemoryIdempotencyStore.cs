using System.Collections.Concurrent;
using Common.Application.Idempotency;

namespace CoreQuant.Commen.UnitTests;

/// <summary>
/// In-process <see cref="IIdempotencyStore"/> for unit tests.
/// </summary>
internal sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, object> _entries = new();

    /// <inheritdoc />
    public Task<(bool Found, TResponse? Response)> TryGetAsync<TResponse>(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_entries.TryGetValue(key, out var value) && value is TResponse typed)
        {
            return Task.FromResult<(bool, TResponse?)>((true, typed));
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
        ArgumentNullException.ThrowIfNull(response);
        _ = ttl;
        _entries[key] = response!;
        return Task.CompletedTask;
    }
}
