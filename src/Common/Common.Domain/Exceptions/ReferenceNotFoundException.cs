namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when a required reference to another aggregate or entity is missing.
/// </summary>
public sealed class ReferenceNotFoundException : DomainException
{
    public ReferenceNotFoundException(string referenceName, object? key = null, Exception? innerException = null)
        : base(
            DomainErrorCode.ReferenceNotFound,
            key is null ? $"Referenced {referenceName} was not found." : $"Referenced {referenceName} '{key}' was not found.",
            innerException,
            value: key,
            entityName: referenceName)
    {
    }
}
