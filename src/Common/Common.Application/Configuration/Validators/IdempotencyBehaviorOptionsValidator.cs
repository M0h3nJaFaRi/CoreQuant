using Common.Application.Configuration.Options;
using Microsoft.Extensions.Options;

namespace Common.Application.Configuration.Validators;

/// <summary>
/// Validates <see cref="IdempotencyBehaviorOptions"/> at options bind / host start.
/// </summary>
internal sealed class IdempotencyBehaviorOptionsValidator : IValidateOptions<IdempotencyBehaviorOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, IdempotencyBehaviorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.DefaultTtl <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail(
                $"{IdempotencyBehaviorOptions.SectionName}:DefaultTtl must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
