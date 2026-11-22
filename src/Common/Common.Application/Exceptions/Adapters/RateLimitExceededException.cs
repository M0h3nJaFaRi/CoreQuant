using Common.Application.Exceptions.Abstraction.Business;
using Common.Application.Exceptions.ErrorCode;

namespace Common.Application.Exceptions.Adapters;

/// <summary>
/// Thrown when a CQRS request exceeds its configured rate limit.
/// </summary>
public sealed class RateLimitExceededException(
    string message,
    TimeSpan? retryAfter = null,
    string? handler = null,
    string? requestType = null)
    : BusinessException(BusinessErrorCode.LimitExceeded, message, handler, requestType)
{
    public TimeSpan? RetryAfter { get; } = retryAfter;
}