using Common.Domain.Exceptions.Abstraction;
using Common.Domain.Exceptions.ErrorCode;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when an expected aggregate or entity cannot be located.
/// </summary>
public sealed class NotFoundException(
    string entityName,
    object? key = null,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainException(
        DomainErrorCode.NotFound,
        key is null ? $"{entityName} was not found." : $"{entityName} '{key}' was not found.",
        value: key,
        entityName: entityName,
        aggregateName: aggregateName,
        innerException: innerException);
