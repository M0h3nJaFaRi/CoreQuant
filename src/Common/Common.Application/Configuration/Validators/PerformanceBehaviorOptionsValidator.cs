using Common.Application.Configuration.Options;
using Microsoft.Extensions.Options;

namespace Common.Application.Configuration.Validators;

/// <summary>
/// Validates <see cref="PerformanceBehaviorOptions"/> at options bind / host start.
/// </summary>
internal sealed class PerformanceBehaviorOptionsValidator : IValidateOptions<PerformanceBehaviorOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, PerformanceBehaviorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.SlowRequestThreshold <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail(
                $"{PerformanceBehaviorOptions.SectionName}:SlowRequestThreshold must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
