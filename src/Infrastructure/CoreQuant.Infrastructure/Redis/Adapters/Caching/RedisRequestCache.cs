using Common.Application.Caching;
using Microsoft.Extensions.Options;
using CoreQuant.Infrastructure.Redis.Core;
using CoreQuant.Infrastructure.Redis.Configuration.Options;

namespace CoreQuant.Infrastructure.Redis.Adapters.Caching;

/// <summary>
/// Redis-backed distributed <see cref="IRequestCache"/> for multi-pod deployments.
/// </summary>
internal sealed class RedisRequestCache(IRedisCore redis, IOptions<CoreQuantRedisOptions> options) : IRequestCache
{
    private readonly uint _database = options.Value.Cache.Database;

    /// <inheritdoc />
    public async Task<(bool Hit, T? Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return await redis.GetAsync<T>(_database, BuildKey(key), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return redis.SetAsync(_database, BuildKey(key), value, ttl, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return redis.DeleteAsync(_database, BuildKey(key), cancellationToken);
    }

    private static string BuildKey(string key) => $"cache:{key.TrimStart(':').Trim()}";
}