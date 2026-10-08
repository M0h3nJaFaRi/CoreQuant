using Common.Application.Idempotency;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.KeyPrefix;
using Framework.Serialization.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CoreQuant.Infrastructure.Redis.Idempotency;

/// <summary>
/// Redis-backed distributed <see cref="IIdempotencyStore"/> for multi-pod deployments.
/// </summary>
internal sealed class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly IJsonSerializer _serializer;
    private readonly string _keyPrefix;

    public RedisIdempotencyStore(
        IConnectionMultiplexer multiplexer,
        IJsonSerializer serializer,
        IOptions<RedisKeyNamespaceOptions> namespaceOptions)
    {
        ArgumentNullException.ThrowIfNull(multiplexer);
        ArgumentNullException.ThrowIfNull(serializer);
        ArgumentNullException.ThrowIfNull(namespaceOptions);

        _multiplexer = multiplexer;
        _serializer = serializer;
        _keyPrefix = RedisKeyPrefixBuilder.BuildFeaturePrefix(
            namespaceOptions.Value,
            RedisKeyPrefixBuilder.IdempotencyFeature);
    }

    /// <inheritdoc />
    public async Task<(bool Found, TResponse? Response)> TryGetAsync<TResponse>(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        var value = await _multiplexer.GetDatabase()
            .StringGetAsync(BuildKey(key))
            .ConfigureAwait(false);

        if (value.IsNullOrEmpty)
        {
            return (false, default);
        }

        return (true, _serializer.Deserialize<TResponse>((string)value!));
    }

    /// <inheritdoc />
    public async Task SetAsync<TResponse>(
        string key,
        TResponse response,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), ttl, "TTL must be positive.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var payload = _serializer.Serialize(response);
        await _multiplexer.GetDatabase()
            .StringSetAsync(BuildKey(key), payload, ttl)
            .ConfigureAwait(false);
    }

    private RedisKey BuildKey(string key) => _keyPrefix + key;
}
