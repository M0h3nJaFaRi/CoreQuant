using Common.Domain.Exceptions.Abstraction;
using Common.Domain.Exceptions.ErrorCode;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when aggregate or relational integrity is violated.
/// </summary>
public sealed class IntegrityViolationException(
    string message,
    string? entityName = null,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainLayerException(
        DomainErrorCode.IntegrityViolation,
        message,
        entityName: entityName,
        aggregateName: aggregateName,
        innerException: innerException);
