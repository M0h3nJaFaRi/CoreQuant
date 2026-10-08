using Microsoft.Extensions.Options;
using CoreQuant.Infrastructure.Redis.Configuration.Options;

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
            failures.Add($"{CoreQuantRedisOptions.SectionName}:Host is required.");

        if (options.Port <= 0)
            failures.Add($"{CoreQuantRedisOptions.SectionName}:Port must be greater than zero.");

        if (string.IsNullOrWhiteSpace(options.HostRole))
            failures.Add($"{CoreQuantRedisOptions.SectionName}:HostRole is required.");

        if (options.Connection.ConnectRetry < 0)
            failures.Add($"{CoreQuantRedisOptions.SectionName}:Connection:ConnectRetry must be zero or greater.");

        if (options.Connection.AsyncTimeout <= TimeSpan.Zero || options.Connection.ConnectTimeout <= TimeSpan.Zero || 
            options.Connection.SyncTimeout <= TimeSpan.Zero || options.Connection.KeepAlive <= TimeSpan.Zero)
            failures.Add($"{CoreQuantRedisOptions.SectionName}:Connection timeouts and KeepAlive must be greater than zero.");

        ValidateDatabase(failures, $"{CoreQuantRedisOptions.SectionName}:Cache:Database", options.Cache.Database);
        ValidateDatabase(failures, $"{CoreQuantRedisOptions.SectionName}:Idempotency:Database", options.Idempotency.Database);
        ValidateDatabase(failures, $"{CoreQuantRedisOptions.SectionName}:RateLimit:Database", options.RateLimit.Database);

        return failures.Count > 0 ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
    }

    private static void ValidateDatabase(List<string> failures, string path, uint database)
    {
        if (database < 0) failures.Add($"{path} must be zero or greater.");
    }
}