using Common.Application.Caching;
using Common.Infrastructure.Configuration.Options;
using Framework.Serialization.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Common.Infrastructure.Caching;

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
        IOptions<RedisRequestCacheOptions> options)
    {
        ArgumentNullException.ThrowIfNull(multiplexer);
        ArgumentNullException.ThrowIfNull(serializer);
        ArgumentNullException.ThrowIfNull(options);

        _multiplexer = multiplexer;
        _serializer = serializer;
        var prefix = options.Value.KeyPrefix;
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        _keyPrefix = prefix.EndsWith(':') ? prefix : prefix + ":";
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
