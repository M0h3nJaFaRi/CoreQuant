namespace Common.Application.Exceptions;

/// <summary>
/// Thrown when the caller is not allowed to execute a use case.
/// </summary>
public sealed class ForbiddenException : Exception
{
    /// <summary>
    /// Creates a forbidden exception.
    /// </summary>
    public ForbiddenException(string message = "Forbidden.")
        : base(message)
    {
    }
}
