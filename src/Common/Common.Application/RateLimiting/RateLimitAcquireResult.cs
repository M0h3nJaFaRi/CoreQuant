namespace Common.Application.RateLimiting;

/// <summary>
/// Outcome of a rate-limit acquisition attempt.
/// </summary>
/// <param name="Acquired">Whether the permit was granted.</param>
/// <param name="RetryAfter">Suggested wait before retry when not acquired.</param>
public sealed record RateLimitAcquireResult(bool Acquired, TimeSpan? RetryAfter);