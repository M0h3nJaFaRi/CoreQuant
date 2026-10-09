using CoreQuant.Infrastructure.Redis.Core;
using CoreQuant.Infrastructure.Redis.Core.Contract;

namespace CoreQuant.Infrastructure.Redis.Core.Scripts;

/// <summary>
/// Shared Redis Lua scripts executed through <see cref="IRedisCore"/>.
/// </summary>
internal static class RedisScripts
{
    /// <summary>
    /// Sliding-window rate limit acquisition script.
    /// Returns { acquired (0|1), retryAfterMs }.
    /// </summary>
    public static readonly RedisScript SlidingWindowRateLimit = RedisScript.Prepare(
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

    /// <summary>
    /// Atomically returns and deletes a string value. Returns nil when missing.
    /// </summary>
    public static readonly RedisScript GetDelString = RedisScript.Prepare(
        """
        local value = redis.call('GET', @key)
        if value then
            redis.call('DEL', @key)
        end
        return value
        """);

    /// <summary>
    /// Releases a lock when the owner token matches. Returns 1 when deleted, 0 otherwise.
    /// </summary>
    public static readonly RedisScript ReleaseLock = RedisScript.Prepare(
        """
        if redis.call('GET', @key) == @token then
            return redis.call('DEL', @key)
        end
        return 0
        """);
}