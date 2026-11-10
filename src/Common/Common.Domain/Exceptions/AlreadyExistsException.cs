namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when creating a resource that already exists.
/// </summary>
public sealed class AlreadyExistsException : DomainException
{
    public AlreadyExistsException(string entityName, object? key = null, Exception? innerException = null)
        : base(
            DomainErrorCode.AlreadyExists,
            key is null ? $"{entityName} already exists." : $"{entityName} '{key}' already exists.",
            innerException,
            value: key,
            entityName: entityName)
    {
    }
}
