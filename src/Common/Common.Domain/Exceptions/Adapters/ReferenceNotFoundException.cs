using Common.Domain.Exceptions.Abstraction;
using Common.Domain.Exceptions.ErrorCode;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when a required reference to another aggregate or entity is missing.
/// </summary>
public sealed class ReferenceNotFoundException(
    string referenceName,
    object? key = null,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainException(
        DomainErrorCode.ReferenceNotFound,
        key is null ? $"Referenced {referenceName} was not found." : $"Referenced {referenceName} '{key}' was not found.",
        value: key,
        entityName: referenceName,
        aggregateName: aggregateName,
        innerException: innerException);
