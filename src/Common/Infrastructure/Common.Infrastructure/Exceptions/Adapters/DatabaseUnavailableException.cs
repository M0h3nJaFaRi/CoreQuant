using Common.Infrastructure.Exceptions.Abstraction;
using Common.Infrastructure.Exceptions.ErrorCode;

namespace Common.Infrastructure.Exceptions.Adapters;

/// <summary>
/// Thrown when the database is unavailable.
/// </summary>
public sealed class DatabaseUnavailableException(string message, Exception? innerException = null)
    : InfrastructureException(InfrastructureErrorCode.DatabaseUnavailable, message, innerException);
