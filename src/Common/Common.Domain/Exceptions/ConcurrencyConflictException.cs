namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when a concurrent modification conflict is detected.
/// </summary>
public sealed class ConcurrencyConflictException : DomainException
{
    public ConcurrencyConflictException(string entityName, object? key = null, Exception? innerException = null)
        : base(
            DomainErrorCode.ConcurrencyConflict,
            key is null
                ? $"{entityName} was modified by another process."
                : $"{entityName} '{key}' was modified by another process.",
            innerException,
            value: key,
            entityName: entityName)
    {
    }
}
