namespace Common.Application.Exceptions;

/// <summary>
/// A single validation failure.
/// </summary>
public sealed record ValidationFailure(string PropertyName, string ErrorMessage, string? ErrorCode = null);
