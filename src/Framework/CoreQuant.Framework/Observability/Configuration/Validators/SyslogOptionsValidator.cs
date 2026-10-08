using Framework.Observability.Configuration.Options;
using Framework.Observability.Logging;
using Microsoft.Extensions.Options;

namespace Framework.Observability.Configuration.Validators;

/// <summary>
/// Validates <see cref="SyslogOptions"/> at options bind / host start.
/// </summary>
public sealed class SyslogOptionsValidator : IValidateOptions<SyslogOptions>
{
    private static readonly HashSet<string> AllowedFormats =
        new(StringComparer.OrdinalIgnoreCase) { "RFC5424", "RFC3164" };

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, SyslogOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (!LogLevelParser.TryParse(options.RestrictedToMinimumLevel, out _))
        {
            failures.Add(
                $"{SyslogOptions.SectionName}:RestrictedToMinimumLevel is not a valid log level.");
        }

        if (!AllowedFormats.Contains(options.Format))
        {
            failures.Add(
                $"{SyslogOptions.SectionName}:Format must be one of: {string.Join(", ", AllowedFormats)}.");
        }

        if (options.Enabled)
        {
            if (string.IsNullOrWhiteSpace(options.Address))
            {
                failures.Add($"{SyslogOptions.SectionName}:Address is required when Enabled is true.");
            }

            if (options.Port <= 0 || options.Port > 65535)
            {
                failures.Add($"{SyslogOptions.SectionName}:Port must be between 1 and 65535 when Enabled is true.");
            }
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
