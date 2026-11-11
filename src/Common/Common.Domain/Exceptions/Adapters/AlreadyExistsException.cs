using Common.Domain.Exceptions.Abstraction;
using Common.Domain.Exceptions.ErrorCode;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when creating a resource that already exists.
/// </summary>
public sealed class AlreadyExistsException(
    string entityName,
    object? key = null,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainException(
        DomainErrorCode.AlreadyExists,
        key is null ? $"{entityName} already exists." : $"{entityName} '{key}' already exists.",
        value: key,
        entityName: entityName,
        aggregateName: aggregateName,
        innerException: innerException);
