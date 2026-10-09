using Common.Application.Tokens;
using Microsoft.Extensions.Options;
using CoreQuant.Infrastructure.Redis.Core;
using CoreQuant.Infrastructure.Redis.Configuration.Options;

namespace CoreQuant.Infrastructure.Redis.Adapters.Tokens;

/// <summary>
/// Redis-backed implementation of <see cref="IRedisTokenStore"/>.
/// </summary>
internal sealed class RedisTokenStore(IRedisCore redis, IOptions<CoreQuantRedisOptions> options) : IRedisTokenStore
{
    private readonly uint _database = options.Value.Token.Database;

    /// <inheritdoc />
    public Task StoreAsync<T>(string provider, string key, T tokenModel, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tokenModel);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);

        if (ttl <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(ttl), ttl, "TTL must be positive.");

        return redis.SetAsync(_database, BuildKey(provider, key), tokenModel, ttl, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(bool Found, T? Token)> TryGetAsync<T>(string provider, string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);

        var (found, token) = await redis.GetAsync<T>(_database, BuildKey(provider, key), cancellationToken).ConfigureAwait(false);

        return (found, token);
    }

    /// <inheritdoc />
    public async Task<(bool Consumed, T? TokenModel)> TryConsumeAsync<T>(string provider, string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var (removed, TokenModel) = await redis.TryRemoveAsync<T>(_database, BuildKey(provider, key), cancellationToken).ConfigureAwait(false);

        return (removed, TokenModel);
    }

    /// <inheritdoc />
    public Task<bool> RevokeAsync(string provider, string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return redis.RemoveAsync(_database, BuildKey(provider, key), cancellationToken);
    }

    private static string BuildKey(string provider, string key) => $"token:{provider.TrimStart(':').Trim()}:{key.TrimStart(':').Trim()}";
}