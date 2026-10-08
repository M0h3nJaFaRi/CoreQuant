using Common.Application.Caching;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.KeyPrefix;
using Framework.Serialization.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CoreQuant.Infrastructure.Redis.Caching;

/// <summary>
/// Redis-backed distributed <see cref="IRequestCache"/> for multi-pod deployments.
/// </summary>
internal sealed class RedisRequestCache : IRequestCache
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly IJsonSerializer _serializer;
    private readonly string _keyPrefix;

    public RedisRequestCache(
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
            RedisKeyPrefixBuilder.CacheFeature);
    }

    /// <inheritdoc />
    public async Task<(bool Hit, T? Value)> TryGetAsync<T>(
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

        return (true, _serializer.Deserialize<T>((string)value!));
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be positive.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var payload = _serializer.Serialize(value);
        await _multiplexer.GetDatabase()
            .StringSetAsync(BuildKey(key), payload, duration)
            .ConfigureAwait(false);
    }

    private RedisKey BuildKey(string key) => _keyPrefix + key;
}
