namespace Common.Application.RateLimiting;

/// <summary>
/// Acquires rate-limit permits for CQRS requests.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Attempts to acquire one permit for <paramref name="partitionKey"/> within the configured window.
    /// </summary>
    Task<RateLimitAcquireResult> TryAcquireAsync(
        string partitionKey,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Outcome of a rate-limit acquisition attempt.
/// </summary>
/// <param name="Acquired">Whether the permit was granted.</param>
/// <param name="RetryAfter">Suggested wait before retry when not acquired.</param>
public sealed record RateLimitAcquireResult(bool Acquired, TimeSpan? RetryAfter);
