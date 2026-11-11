using Common.Domain.Exceptions.Abstraction;
using Common.Domain.Exceptions.ErrorCode;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when an aggregate or entity is in an invalid state for the attempted operation.
/// </summary>
public sealed class InvalidStateException(
    string entityName,
    string message,
    object? current = null,
    object? expected = null,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainException(
        DomainErrorCode.InvalidState,
        message,
        value: current,
        entityName: entityName,
        aggregateName: aggregateName,
        innerException: innerException)
{
    public object? Expected { get; } = expected;
}
