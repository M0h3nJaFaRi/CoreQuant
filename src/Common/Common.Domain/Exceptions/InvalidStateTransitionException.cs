namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when a state transition is not allowed by domain rules.
/// </summary>
public sealed class InvalidStateTransitionException : DomainException
{
    public InvalidStateTransitionException(
        string entityName,
        object from,
        object to,
        Exception? innerException = null)
        : base(
            DomainErrorCode.InvalidStateTransition,
            $"{entityName} cannot transition from '{from}' to '{to}'.",
            innerException,
            entityName: entityName)
    {
        From = from;
        To = to;
    }

    public object From { get; }

    public object To { get; }
}
