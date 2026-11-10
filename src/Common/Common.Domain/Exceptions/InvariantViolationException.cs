namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when a domain invariant is violated.
/// </summary>
public sealed class InvariantViolationException : DomainException
{
    public InvariantViolationException(string message, Exception? innerException = null)
        : base(DomainErrorCode.InvariantViolation, message, innerException)
    {
    }
}
