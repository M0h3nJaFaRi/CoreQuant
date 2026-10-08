using CoreQuant.Infrastructure.Redis.Configuration.Options;
using Microsoft.Extensions.Options;

namespace CoreQuant.Infrastructure.Redis.Configuration.Validators;

/// <summary>
/// Validates <see cref="CoreQuantRedisOptions"/> at options bind / host start.
/// </summary>
internal sealed class CoreQuantRedisOptionsValidator : IValidateOptions<CoreQuantRedisOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, CoreQuantRedisOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            failures.Add($"{CoreQuantRedisOptions.SectionName}:Host is required.");
        }

        if (options.Port <= 0)
        {
            failures.Add($"{CoreQuantRedisOptions.SectionName}:Port must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(options.HostIdentity))
        {
            failures.Add($"{CoreQuantRedisOptions.SectionName}:HostIdentity is required.");
        }

        if (options.Connection.ConnectRetry < 0)
        {
            failures.Add($"{CoreQuantRedisOptions.SectionName}:Connection:ConnectRetry must be zero or greater.");
        }

        if (options.Connection.AsyncTimeout <= TimeSpan.Zero
            || options.Connection.ConnectTimeout <= TimeSpan.Zero
            || options.Connection.SyncTimeout <= TimeSpan.Zero
            || options.Connection.KeepAlive <= TimeSpan.Zero)
        {
            failures.Add(
                $"{CoreQuantRedisOptions.SectionName}:Connection timeouts and KeepAlive must be greater than zero.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
