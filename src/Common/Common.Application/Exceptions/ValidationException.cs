namespace Common.Application.Exceptions;

/// <summary>
/// Thrown when FluentValidation rejects a command or query.
/// </summary>
public sealed class ValidationException(IReadOnlyCollection<ValidationFailure> failures) : Exception("One or more validation failures occurred.")
{
    /// <summary>
    /// Validation failures.
    /// </summary>
    public IReadOnlyCollection<ValidationFailure> Failures { get; } = failures;
}