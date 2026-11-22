using Common.Domain.Exceptions.Abstraction;
using Common.Domain.Exceptions.ErrorCode;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when a time-bound domain resource has expired.
/// </summary>
public sealed class ExpiredException(
    string entityName,
    object? key = null,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainLayerException(
        DomainErrorCode.Expired,
        key is null ? $"{entityName} has expired." : $"{entityName} '{key}' has expired.",
        value: key,
        entityName: entityName,
        aggregateName: aggregateName,
        innerException: innerException);
