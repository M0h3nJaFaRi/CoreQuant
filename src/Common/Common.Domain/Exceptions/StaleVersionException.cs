namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when an optimistic concurrency version is stale.
/// </summary>
public sealed class StaleVersionException : DomainException
{
    public StaleVersionException(string entityName, object? key = null, Exception? innerException = null)
        : base(
            DomainErrorCode.StaleVersion,
            key is null ? $"{entityName} version is stale." : $"{entityName} '{key}' version is stale.",
            innerException,
            value: key,
            entityName: entityName)
    {
    }
}
