using Common.Domain.Exceptions.Abstraction;
using Common.Domain.Exceptions.ErrorCode;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when a duplicate value is detected within domain rules.
/// </summary>
public sealed class DuplicateException(
    string entityName,
    object? key = null,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainLayerException(
        DomainErrorCode.Duplicate,
        key is null ? $"Duplicate {entityName}." : $"Duplicate {entityName} '{key}'.",
        value: key,
        entityName: entityName,
        aggregateName: aggregateName,
        innerException: innerException);
