using Common.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;

namespace Common.Infrastructure.Configuration.Validators;

/// <summary>
/// Validates <see cref="RedisRequestCacheOptions"/> at options bind / host start.
/// </summary>
internal sealed class RedisRequestCacheOptionsValidator : IValidateOptions<RedisRequestCacheOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, RedisRequestCacheOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.KeyPrefix))
        {
            return ValidateOptionsResult.Fail(
                $"{RedisRequestCacheOptions.SectionName}:KeyPrefix is required.");
        }

        return ValidateOptionsResult.Success;
    }
}
