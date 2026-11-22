using Common.Domain.Exceptions.Abstraction;
using Common.Domain.Exceptions.ErrorCode;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when an optimistic concurrency version is stale.
/// </summary>
public sealed class StaleVersionException(
    string entityName,
    object? key = null,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainLayerException(
        DomainErrorCode.StaleVersion,
        key is null ? $"{entityName} version is stale." : $"{entityName} '{key}' version is stale.",
        value: key,
        entityName: entityName,
        aggregateName: aggregateName,
        innerException: innerException);
