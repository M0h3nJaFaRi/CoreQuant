namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when a domain invariant is violated.
/// </summary>
public class DomainException(string message, string code) : Exception(message)
{
    /// <summary>
    /// Machine-readable error code (e.g. <c>product_not_ready</c>).
    /// </summary>
    public string Code { get; } = code.Trim();
}