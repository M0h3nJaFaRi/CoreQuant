using Common.Infrastructure.Exceptions.Abstraction;
using Common.Infrastructure.Exceptions.ErrorCode;

namespace Common.Infrastructure.Exceptions.Adapters;

/// <summary>
/// Thrown for uncategorized infrastructure failures.
/// </summary>
public sealed class UnexpectedInfrastructureFailureException(string message, Exception? innerException = null)
    : InfrastructureLayerException(InfrastructureErrorCode.UnexpectedInfrastructureFailure, message, innerException);
