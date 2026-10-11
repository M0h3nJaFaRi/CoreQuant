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
        var prefix = DatabaseOptions.SectionName;

        if (!options.IsConfigured)
        {
            failures.Add($"{prefix}:Host and Name are required.");
        }

        if (options.Port <= 0)
        {
            failures.Add($"{prefix}:Port must be greater than zero.");
        }

        if (options.CommandTimeout is { } timeout && timeout <= TimeSpan.Zero)
        {
            failures.Add($"{prefix}:CommandTimeout must be greater than zero when set.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
