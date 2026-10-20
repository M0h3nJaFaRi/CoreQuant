namespace Common.Application.Exceptions;

/// <summary>
/// Thrown when a CQRS request exceeds its configured rate limit.
/// </summary>
public sealed class RateLimitExceededException(string message, TimeSpan? retryAfter = null) : Exception(message)
{
    /// <summary>
    /// Suggested wait time before retrying, when available.
    /// </summary>
    public TimeSpan? RetryAfter { get; } = retryAfter;
}