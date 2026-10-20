namespace Common.Application.Exceptions;

/// <summary>
/// Thrown when the caller is not allowed to execute a use case.
/// </summary>
internal sealed class ForbiddenException(string message = "Forbidden.", string? reason = null) : Exception(message)
{
    public string? Reason { get; } = reason;
}