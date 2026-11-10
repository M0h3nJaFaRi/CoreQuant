namespace Common.Framework.Exceptions;

/// <summary>
/// Minimal contract for exceptions that expose a stable machine-readable error code.
/// </summary>
public interface IHasErrorCode
{
    /// <summary>
    /// Machine-readable error code (UPPER_SNAKE_CASE).
    /// </summary>
    string Code { get; }
}
