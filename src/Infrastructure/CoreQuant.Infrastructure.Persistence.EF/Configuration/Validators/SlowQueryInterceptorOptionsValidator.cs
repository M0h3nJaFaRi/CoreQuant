using Microsoft.Extensions.Options;
using CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;

namespace CoreQuant.Infrastructure.Persistence.EF.Configuration.Validators;

/// <summary>
/// Validates <see cref="SlowQueryInterceptorOptions"/> at options bind / host start.
/// </summary>
internal sealed class SlowQueryInterceptorOptionsValidator : IValidateOptions<SlowQueryInterceptorOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, SlowQueryInterceptorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.WarningThreshold <= TimeSpan.Zero)
        {
            failures.Add($"{SlowQueryInterceptorOptions.SectionName}:WarningThreshold must be greater than zero.");
        }

        if (options.ErrorThreshold <= TimeSpan.Zero)
        {
            failures.Add($"{SlowQueryInterceptorOptions.SectionName}:ErrorThreshold must be greater than zero.");
        }

        if (options.CriticalThreshold <= TimeSpan.Zero)
        {
            failures.Add($"{SlowQueryInterceptorOptions.SectionName}:CriticalThreshold must be greater than zero.");
        }

        if (options.WarningThreshold >= options.ErrorThreshold)
        {
            failures.Add(
                $"{SlowQueryInterceptorOptions.SectionName}:WarningThreshold must be less than ErrorThreshold.");
        }

        if (options.ErrorThreshold >= options.CriticalThreshold)
        {
            failures.Add(
                $"{SlowQueryInterceptorOptions.SectionName}:ErrorThreshold must be less than CriticalThreshold.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
