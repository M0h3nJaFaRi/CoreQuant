using Common.Application.Configuration.Options;
using Microsoft.Extensions.Options;

namespace Common.Application.Configuration.Validators;

/// <summary>
/// Validates <see cref="CachingBehaviorOptions"/> at options bind / host start.
/// </summary>
internal sealed class CachingBehaviorOptionsValidator : IValidateOptions<CachingBehaviorOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, CachingBehaviorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.DefaultDuration <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail(
                $"{CachingBehaviorOptions.SectionName}:DefaultDuration must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
