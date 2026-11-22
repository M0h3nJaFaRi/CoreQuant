using Common.Infrastructure.Exceptions.Abstraction;
using Common.Infrastructure.Exceptions.ErrorCode;

namespace Common.Infrastructure.Exceptions.Adapters;

/// <summary>
/// Thrown when the cache layer is unavailable.
/// </summary>
public sealed class CacheUnavailableException(string message, Exception? innerException = null)
    : InfrastructureLayerException(InfrastructureErrorCode.CacheUnavailable, message, innerException);
