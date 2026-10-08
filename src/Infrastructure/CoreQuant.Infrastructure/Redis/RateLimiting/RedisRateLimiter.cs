using Common.Application.RateLimiting;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.KeyPrefix;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CoreQuant.Infrastructure.Redis.RateLimiting;

/// <summary>
/// Redis-backed sliding-window rate limiter for multi-pod deployments.
/// </summary>
internal sealed class RedisRateLimiter : IRateLimiter
{
    private static readonly LuaScript AcquireScript = LuaScript.Prepare(
        """
        local now = tonumber(@now)
        local window = tonumber(@window)
        local limit = tonumber(@limit)

        redis.call('ZREMRANGEBYSCORE', @key, 0, now - window)
        local count = redis.call('ZCARD', @key)
        if count >= limit then
            local oldest = redis.call('ZRANGE', @key, 0, 0, 'WITHSCORES')
            if oldest[2] ~= nil then
                local retry_ms = window - (now - tonumber(oldest[2]))
                if retry_ms < 0 then
                    retry_ms = 0
                end
                return { 0, retry_ms }
            end
            return { 0, window }
        end

        redis.call('ZADD', @key, now, @member)
        redis.call('PEXPIRE', @key, window)
        return { 1, 0 }
        """);

    private readonly IConnectionMultiplexer _multiplexer;
    private readonly string _keyPrefix;

    public RedisRateLimiter(
        IConnectionMultiplexer multiplexer,
        IOptions<RedisKeyNamespaceOptions> namespaceOptions)
    {
        ArgumentNullException.ThrowIfNull(multiplexer);
        ArgumentNullException.ThrowIfNull(namespaceOptions);

        _multiplexer = multiplexer;
        _keyPrefix = RedisKeyPrefixBuilder.BuildFeaturePrefix(
            namespaceOptions.Value,
            RedisKeyPrefixBuilder.RateLimitFeature);
    }

    /// <inheritdoc />
    public async Task<RateLimitAcquireResult> TryAcquireAsync(
        string partitionKey,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        cancellationToken.ThrowIfCancellationRequested();

        if (permitLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(permitLimit), permitLimit, "Permit limit must be greater than zero.");
        }

        if (window <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(window), window, "Window must be greater than zero.");
        }

        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowMs = (long)window.TotalMilliseconds;
        var member = $"{nowMs}:{Guid.NewGuid():N}";

        var result = await _multiplexer.GetDatabase().ScriptEvaluateAsync(
                AcquireScript,
                new
                {
                    key = (RedisKey)(_keyPrefix + partitionKey),
                    now = (RedisValue)nowMs,
                    window = (RedisValue)windowMs,
                    limit = (RedisValue)permitLimit,
                    member = (RedisValue)member
                })
            .ConfigureAwait(false);

        if (result.IsNull)
        {
            return new RateLimitAcquireResult(false, window);
        }

        var values = (RedisResult[])result!;
        var acquired = (int)values[0] == 1;
        var retryAfterMs = (long)values[1];
        TimeSpan? retryAfter = retryAfterMs > 0
            ? TimeSpan.FromMilliseconds(retryAfterMs)
            : null;

        return new RateLimitAcquireResult(acquired, retryAfter);
    }
}
