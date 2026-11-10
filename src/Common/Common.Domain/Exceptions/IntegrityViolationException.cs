namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when aggregate or relational integrity is violated.
/// </summary>
public sealed class IntegrityViolationException : DomainException
{
    public IntegrityViolationException(string message, Exception? innerException = null)
        : base(DomainErrorCode.IntegrityViolation, message, innerException)
    {
    }
}
