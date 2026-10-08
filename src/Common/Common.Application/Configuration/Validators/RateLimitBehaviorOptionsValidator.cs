using Common.Application.Configuration.Options;
using Microsoft.Extensions.Options;

namespace Common.Application.Configuration.Validators;

/// <summary>
/// Validates <see cref="RateLimitBehaviorOptions"/> at options bind / host start.
/// </summary>
internal sealed class RateLimitBehaviorOptionsValidator : IValidateOptions<RateLimitBehaviorOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, RateLimitBehaviorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.DefaultPermitLimit <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"{RateLimitBehaviorOptions.SectionName}:DefaultPermitLimit must be greater than zero.");
        }

        if (options.DefaultWindow <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail(
                $"{RateLimitBehaviorOptions.SectionName}:DefaultWindow must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
