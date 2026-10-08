using Common.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;

namespace Common.Infrastructure.Configuration.Validators;

/// <summary>
/// Validates <see cref="RedisIdempotencyStoreOptions"/> at options bind / host start.
/// </summary>
internal sealed class RedisIdempotencyStoreOptionsValidator : IValidateOptions<RedisIdempotencyStoreOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, RedisIdempotencyStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.KeyPrefix))
        {
            return ValidateOptionsResult.Fail(
                $"{RedisIdempotencyStoreOptions.SectionName}:KeyPrefix is required.");
        }

        return ValidateOptionsResult.Success;
    }
}
