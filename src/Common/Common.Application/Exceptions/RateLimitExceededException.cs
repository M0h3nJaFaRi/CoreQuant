namespace Common.Application.Exceptions;

/// <summary>
/// Thrown when a CQRS request exceeds its configured rate limit.
/// </summary>
public sealed class RateLimitExceededException : Exception
{
    /// <summary>
    /// Creates a rate-limit exception.
    /// </summary>
    public RateLimitExceededException(string message, TimeSpan? retryAfter = null)
        : base(message)
    {
        RetryAfter = retryAfter;
    }

    /// <summary>
    /// Suggested wait time before retrying, when available.
    /// </summary>
    public TimeSpan? RetryAfter { get; }
}
