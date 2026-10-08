namespace Common.Application.RateLimiting;

/// <summary>
/// Acquires rate-limit permits for CQRS requests.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Attempts to acquire one permit for <paramref name="partitionKey"/> within the configured window.
    /// </summary>
    Task<RateLimitAcquireResult> TryAcquireAsync(string partitionKey, uint permitLimit, TimeSpan window, CancellationToken cancellationToken = default);
}