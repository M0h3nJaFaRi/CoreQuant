namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when an expected aggregate or entity cannot be located.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object? key = null, Exception? innerException = null)
        : base(
            DomainErrorCode.NotFound,
            FormatMessage(entityName, key),
            innerException,
            value: key,
            entityName: entityName)
    {
    }

    private static string FormatMessage(string entityName, object? key) =>
        key is null ? $"{entityName} was not found." : $"{entityName} '{key}' was not found.";
}
