namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when a duplicate value is detected within domain rules.
/// </summary>
public sealed class DuplicateException : DomainException
{
    public DuplicateException(string entityName, object? key = null, Exception? innerException = null)
        : base(
            DomainErrorCode.Duplicate,
            key is null ? $"Duplicate {entityName}." : $"Duplicate {entityName} '{key}'.",
            innerException,
            value: key,
            entityName: entityName)
    {
    }
}
