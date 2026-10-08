using Microsoft.Extensions.Options;
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
        ValidateEndpoint(options.Write, $"{DatabaseOptions.SectionName}:Write", required: true, failures);

        var readHasAnyValue =
            !string.IsNullOrWhiteSpace(options.Read.Host)
            || !string.IsNullOrWhiteSpace(options.Read.Name)
            || !string.IsNullOrWhiteSpace(options.Read.Username)
            || !string.IsNullOrWhiteSpace(options.Read.Password);

        if (readHasAnyValue || options.Read.IsConfigured)
        {
            ValidateEndpoint(options.Read, $"{DatabaseOptions.SectionName}:Read", required: true, failures);
        }
        else if (options.Read.Port <= 0)
        {
            failures.Add($"{DatabaseOptions.SectionName}:Read:Port must be greater than zero.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static void ValidateEndpoint(
        DatabaseEndpointOptions endpoint,
        string prefix,
        bool required,
        List<string> failures)
    {
        if (required && !endpoint.IsConfigured)
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
