namespace Common.Infrastructure.Exceptions;

/// <summary>
/// Thrown when a database operation fails unexpectedly.
/// </summary>
public sealed class DatabaseFailureException(string message, Exception? innerException = null)
    : InfrastructureException(InfrastructureErrorCode.DatabaseFailure, message, innerException);
