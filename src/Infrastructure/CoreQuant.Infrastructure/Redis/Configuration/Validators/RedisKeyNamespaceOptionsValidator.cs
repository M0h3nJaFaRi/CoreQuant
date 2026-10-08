using CoreQuant.Infrastructure.Redis.Configuration.Options;
using Microsoft.Extensions.Options;

namespace CoreQuant.Infrastructure.Redis.Configuration.Validators;

/// <summary>
/// Validates <see cref="RedisKeyNamespaceOptions"/> at options bind / host start.
/// </summary>
internal sealed class RedisKeyNamespaceOptionsValidator : IValidateOptions<RedisKeyNamespaceOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, RedisKeyNamespaceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Service))
        {
            failures.Add("Redis key namespace:Service is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            failures.Add("Redis key namespace:Host is required.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
