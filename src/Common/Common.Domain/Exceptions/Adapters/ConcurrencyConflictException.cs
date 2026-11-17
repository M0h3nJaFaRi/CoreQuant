using Common.Domain.Exceptions.Abstraction;
using Common.Domain.Exceptions.ErrorCode;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when a concurrent modification conflict is detected.
/// </summary>
public sealed class ConcurrencyConflictException(
    string entityName,
    object? key = null,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainLayerException(
        DomainErrorCode.ConcurrencyConflict,
        key is null
            ? $"{entityName} was modified by another process."
            : $"{entityName} '{key}' was modified by another process.",
        value: key,
        entityName: entityName,
        aggregateName: aggregateName,
        innerException: innerException);
