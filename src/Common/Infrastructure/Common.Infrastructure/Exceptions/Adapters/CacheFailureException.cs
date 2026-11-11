using Common.Infrastructure.Exceptions.Abstraction;
using Common.Infrastructure.Exceptions.ErrorCode;

namespace Common.Infrastructure.Exceptions.Adapters;

/// <summary>
/// Thrown when a cache operation fails unexpectedly.
/// </summary>
public sealed class CacheFailureException(string message, Exception? innerException = null)
    : InfrastructureException(InfrastructureErrorCode.CacheFailure, message, innerException);
