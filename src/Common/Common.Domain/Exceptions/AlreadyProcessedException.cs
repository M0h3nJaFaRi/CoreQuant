namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when an operation targets an entity that was already processed.
/// </summary>
public sealed class AlreadyProcessedException : DomainException
{
    public AlreadyProcessedException(string entityName, object? key = null, Exception? innerException = null)
        : base(
            DomainErrorCode.AlreadyProcessed,
            key is null ? $"{entityName} has already been processed." : $"{entityName} '{key}' has already been processed.",
            innerException,
            value: key,
            entityName: entityName)
    {
    }
}
