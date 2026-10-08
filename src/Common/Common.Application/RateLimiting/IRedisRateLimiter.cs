namespace Common.Application.RateLimiting;

/// <summary>
/// Redis-backed rate limiter for CQRS requests in multi-pod deployments.
/// </summary>
public interface IRedisRateLimiter
{
    /// <summary>
    /// Attempts to acquire one permit for <paramref name="partitionKey"/> within the configured window.
    /// </summary>
    Task<RateLimitAcquireResult> TryAcquireAsync(string partitionKey, uint permitLimit, TimeSpan window, CancellationToken cancellationToken = default);
}
