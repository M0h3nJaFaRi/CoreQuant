using Common.Domain.Exceptions.Abstraction;
using Common.Domain.Exceptions.ErrorCode;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when an operation targets an entity that was already processed.
/// </summary>
public sealed class AlreadyProcessedException(
    string entityName,
    object? key = null,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainException(
        DomainErrorCode.AlreadyProcessed,
        key is null ? $"{entityName} has already been processed." : $"{entityName} '{key}' has already been processed.",
        value: key,
        entityName: entityName,
        aggregateName: aggregateName,
        innerException: innerException);
