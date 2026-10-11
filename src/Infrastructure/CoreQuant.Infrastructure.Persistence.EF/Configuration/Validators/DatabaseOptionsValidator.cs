using Microsoft.Extensions.Options;
using CoreQuant.Infrastructure.Persistence.EF.Configuration;
using CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;

namespace CoreQuant.Infrastructure.Persistence.EF.Configuration.Validators;

/// <summary>
/// Validates <see cref="DatabaseOptions"/> at options bind / host start.
/// </summary>
internal sealed class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        foreach (var registration in CoreQuantDbContextRegistration.All)
        {
            if (!options.TryGetValue(registration.ConfigurationKey, out var endpoint))
            {
                failures.Add($"{DatabaseOptions.SectionName}:{registration.ConfigurationKey} is required.");
                continue;
            }

            ValidateEndpoint(endpoint, $"{DatabaseOptions.SectionName}:{registration.ConfigurationKey}", failures);
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static void ValidateEndpoint(
        DatabaseEndpointOptions endpoint,
        string prefix,
        List<string> failures)
    {
        if (!endpoint.IsConfigured)
        {
            failures.Add($"{prefix}:Host and Name are required.");
        }

        if (endpoint.Port <= 0)
        {
            failures.Add($"{prefix}:Port must be greater than zero.");
        }

        if (endpoint.CommandTimeout is { } timeout && timeout <= TimeSpan.Zero)
        {
            failures.Add($"{prefix}:CommandTimeout must be greater than zero when set.");
        }
    }
}
