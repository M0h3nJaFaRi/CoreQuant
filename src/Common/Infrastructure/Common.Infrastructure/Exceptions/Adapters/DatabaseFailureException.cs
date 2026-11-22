using Common.Infrastructure.Exceptions.Abstraction;
using Common.Infrastructure.Exceptions.ErrorCode;

namespace Common.Infrastructure.Exceptions.Adapters;

/// <summary>
/// Thrown when a database operation fails unexpectedly.
/// </summary>
public sealed class DatabaseFailureException(string message, Exception? innerException = null)
    : InfrastructureLayerException(InfrastructureErrorCode.DatabaseFailure, message, innerException);
