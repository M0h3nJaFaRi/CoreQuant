using Framework.Observability.Configuration.Options;
using Microsoft.Extensions.Options;

namespace Framework.Observability.Configuration.Validators;

/// <summary>
/// Validates <see cref="FrameworkObservabilityOptions"/> at options bind / host start.
/// </summary>
internal sealed class FrameworkObservabilityOptionsValidator : IValidateOptions<FrameworkObservabilityOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, FrameworkObservabilityOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(FrameworkObservabilityOptions)}:{nameof(FrameworkObservabilityOptions.ServiceName)} is required.");
        }

        return ValidateOptionsResult.Success;
    }
}
