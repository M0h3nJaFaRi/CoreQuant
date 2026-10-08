namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when a required domain object cannot be found.
/// </summary>
public sealed class NotFoundException : DomainException
{
    /// <summary>
    /// Creates a not-found exception.
    /// </summary>
    public NotFoundException(string message, string code = "not_found")
        : base(message, code)
    {
    }
}
