using Common.Domain.Exceptions.Abstraction;
using Common.Domain.Exceptions.ErrorCode;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when a domain invariant is violated.
/// </summary>
public sealed class InvariantViolationException(
    string message,
    string? entityName = null,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainLayerException(
        DomainErrorCode.InvariantViolation,
        message,
        entityName: entityName,
        aggregateName: aggregateName,
        innerException: innerException);
