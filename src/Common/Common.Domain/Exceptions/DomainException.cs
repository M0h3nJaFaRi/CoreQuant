namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when a domain invariant is violated.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Creates a domain exception with a stable error <paramref name="code"/>.
    /// </summary>
    public DomainException(string message, string code)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    /// <summary>
    /// Machine-readable error code (e.g. <c>product_not_ready</c>).
    /// </summary>
    public string Code { get; }
}
