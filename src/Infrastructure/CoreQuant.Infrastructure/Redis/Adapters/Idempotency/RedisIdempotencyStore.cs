using Microsoft.Extensions.Options;
using Common.Application.Idempotency;
using CoreQuant.Infrastructure.Redis.Core;
using CoreQuant.Infrastructure.Redis.Configuration.Options;

namespace CoreQuant.Infrastructure.Redis.Adapters.Idempotency;

/// <summary>
/// Redis-backed distributed <see cref="IIdempotencyStore"/> for multi-pod deployments.
/// </summary>
internal sealed class RedisIdempotencyStore(IRedisCore redis, IOptions<CoreQuantRedisOptions> options) : IIdempotencyStore
{
    private readonly uint _database = options.Value.Idempotency.Database;

    /// <inheritdoc />
    public async Task<(bool Found, TResponse? Response)> TryGetAsync<TResponse>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var (found, value) = await redis.GetAsync<TResponse>(_database, BuildKey(key), cancellationToken).ConfigureAwait(false);

        return (found, value);
    }

    /// <inheritdoc />
    public Task SetAsync<TResponse>(string key, TResponse response, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return redis.SetAsync(_database, BuildKey(key), response, ttl, cancellationToken);
    }

    private static string BuildKey(string key) => $"idempotency:{key}";
}