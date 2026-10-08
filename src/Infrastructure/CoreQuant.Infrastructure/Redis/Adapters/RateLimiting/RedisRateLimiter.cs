using Microsoft.Extensions.Options;
using Common.Application.RateLimiting;
using CoreQuant.Infrastructure.Redis.Core;
using CoreQuant.Infrastructure.Redis.Core.Scripts;
using CoreQuant.Infrastructure.Redis.Configuration.Options;

namespace CoreQuant.Infrastructure.Redis.Adapters.RateLimiting;

/// <summary>
/// Redis-backed sliding-window rate limiter for multi-pod deployments.
/// </summary>
internal sealed class RedisRateLimiter(IRedisCore redis, IOptions<CoreQuantRedisOptions> options) : IRedisRateLimiter
{
    private readonly uint _database = options.Value.RateLimit.Database;

    /// <inheritdoc />
    public async Task<RateLimitAcquireResult> TryAcquireAsync(string partitionKey, uint permitLimit, TimeSpan window, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        cancellationToken.ThrowIfCancellationRequested();


        if (window <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(window), window, "Window must be greater than zero.");

        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var parameter = new Dictionary<string, object?>
        {
            ["now"] = nowMs,
            ["limit"] = (int)permitLimit,
            ["key"] = BuildKey(partitionKey),
            ["window"] = (long)window.TotalMilliseconds,
            ["member"] = $"{nowMs}:{Guid.NewGuid():N}"
        };


        var result = await redis.ExecuteScriptAsync(_database, RedisScripts.SlidingWindowRateLimit, parameter, cancellationToken).ConfigureAwait(false);

        if (result.IsNull) return new RateLimitAcquireResult(false, window);

        var values = result.AsArray();
        var acquired = values[0].AsInt32() == 1;
        var retryAfterMs = values[1].AsInt64();
        TimeSpan? retryAfter = retryAfterMs > 0 ? TimeSpan.FromMilliseconds(retryAfterMs) : null;

        return new RateLimitAcquireResult(acquired, retryAfter);
    }

    private static string BuildKey(string partitionKey) => $"ratelimit:{partitionKey}";
}