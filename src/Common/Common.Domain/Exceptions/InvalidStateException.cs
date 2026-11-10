namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when an aggregate or entity is in an invalid state for the attempted operation.
/// </summary>
public sealed class InvalidStateException : DomainException
{
    public InvalidStateException(
        string entityName,
        string message,
        object? current = null,
        object? expected = null,
        Exception? innerException = null)
        : base(
            DomainErrorCode.InvalidState,
            message,
            innerException,
            value: current,
            entityName: entityName)
    {
        Expected = expected;
    }

    public object? Expected { get; }
}
