using Common.Domain.Exceptions.Abstraction;
using Common.Domain.Exceptions.ErrorCode;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when a state transition is not allowed by domain rules.
/// </summary>
public sealed class InvalidStateTransitionException(
    string entityName,
    object from,
    object to,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainException(
        DomainErrorCode.InvalidStateTransition,
        $"{entityName} cannot transition from '{from}' to '{to}'.",
        value: from,
        entityName: entityName,
        aggregateName: aggregateName,
        innerException: innerException)
{
    public object From { get; } = from;

    public object To { get; } = to;
}
