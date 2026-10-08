namespace Common.Application.Exceptions;

/// <summary>
/// Thrown when FluentValidation rejects a command or query.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Creates a validation exception.
    /// </summary>
    public ValidationException(IReadOnlyCollection<ValidationFailure> failures)
        : base("One or more validation failures occurred.")
    {
        Failures = failures;
    }

    /// <summary>
    /// Validation failures.
    /// </summary>
    public IReadOnlyCollection<ValidationFailure> Failures { get; }
}
