using Framework.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;

namespace Framework.Infrastructure.Configuration.Validators;

/// <summary>
/// Validates <see cref="FrameworkRedisOptions"/> at options bind / host start.
/// </summary>
internal sealed class FrameworkRedisOptionsValidator : IValidateOptions<FrameworkRedisOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, FrameworkRedisOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            failures.Add($"{FrameworkRedisOptions.SectionName}:Host is required.");
        }

        if (options.Port <= 0)
        {
            failures.Add($"{FrameworkRedisOptions.SectionName}:Port must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(options.KeyPrefix))
        {
            failures.Add($"{FrameworkRedisOptions.SectionName}:KeyPrefix is required.");
        }

        if (options.Connection.ConnectRetry < 0)
        {
            failures.Add($"{FrameworkRedisOptions.SectionName}:Connection:ConnectRetry must be zero or greater.");
        }

        if (options.Connection.AsyncTimeout <= TimeSpan.Zero
            || options.Connection.ConnectTimeout <= TimeSpan.Zero
            || options.Connection.SyncTimeout <= TimeSpan.Zero
            || options.Connection.KeepAlive <= TimeSpan.Zero)
        {
            failures.Add(
                $"{FrameworkRedisOptions.SectionName}:Connection timeouts and KeepAlive must be greater than zero.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
