namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when a time-bound domain resource has expired.
/// </summary>
public sealed class ExpiredException : DomainException
{
    public ExpiredException(string entityName, object? key = null, Exception? innerException = null)
        : base(
            DomainErrorCode.Expired,
            key is null ? $"{entityName} has expired." : $"{entityName} '{key}' has expired.",
            innerException,
            value: key,
            entityName: entityName)
    {
    }
}
