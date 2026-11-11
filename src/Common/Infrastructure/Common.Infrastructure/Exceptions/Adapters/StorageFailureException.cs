using Common.Infrastructure.Exceptions.Abstraction;
using Common.Infrastructure.Exceptions.ErrorCode;

namespace Common.Infrastructure.Exceptions.Adapters;

/// <summary>
/// Thrown when object storage fails unexpectedly.
/// </summary>
public sealed class StorageFailureException(string message, Exception? innerException = null)
    : InfrastructureException(InfrastructureErrorCode.StorageFailure, message, innerException);
