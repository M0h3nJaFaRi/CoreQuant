using Common.Framework.Exceptions;

namespace Common.Application.Exceptions;

/// <summary>
/// Thrown when FluentValidation rejects a command or query.
/// </summary>
public sealed class ValidationException : Exception, IHasErrorCode
{
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation failures occurred.")
    {
        Errors = errors;
        Failures = [];
    }

    public ValidationException(IReadOnlyCollection<ValidationFailure> failures)
        : this(ToDictionary(failures))
    {
        Failures = failures;
    }

    /// <summary>
    /// Validation failures (property-level detail).
    /// </summary>
    public IReadOnlyCollection<ValidationFailure> Failures { get; }

    /// <summary>
    /// Errors grouped by property name for API consumers.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public string Code => ValidationErrorCode.ValidationFailed;

    private static IReadOnlyDictionary<string, string[]> ToDictionary(IReadOnlyCollection<ValidationFailure> failures)
    {
        return failures
            .GroupBy(f => f.PropertyName, StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray(),
                StringComparer.Ordinal);
    }
}
