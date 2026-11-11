using Common.Application.Exceptions.Abstraction;
using Common.Application.Exceptions.ErrorCode;

namespace Common.Application.Exceptions.Adapters;

/// <summary>
/// Thrown when a CQRS request exceeds its configured rate limit.
/// </summary>
public sealed class RateLimitExceededException(string message, TimeSpan? retryAfter = null)
    : BusinessException(BusinessErrorCode.LimitExceeded, message)
{
    public TimeSpan? RetryAfter { get; } = retryAfter;
}
