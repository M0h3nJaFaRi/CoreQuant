using Common.Application.Exceptions.Abstraction.Validation;
using Common.Application.Exceptions.ErrorCode;

namespace Common.Application.Exceptions.Adapters;

/// <summary>
/// Thrown when FluentValidation rejects a command or query.
/// </summary>
public sealed class ValidationException : Common.Application.Exceptions.Abstraction.Validation.ApplicationValidationException
{
    public ValidationException(
        IReadOnlyDictionary<string, string[]> errors,
        string? handler = null,
        string? requestType = null)
        : base(ValidationErrorCode.ValidationFailed, "One or more validation failures occurred.", handler, requestType)
    {
        Errors = errors;
        Failures = [];
    }

    public ValidationException(
        IReadOnlyCollection<ValidationFailure> failures,
        string? handler = null,
        string? requestType = null)
        : this(ToDictionary(failures), handler, requestType)
    {
        Failures = failures;
    }

    public IReadOnlyCollection<ValidationFailure> Failures { get; }

    public override IReadOnlyDictionary<string, string[]> Errors { get; }

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