using Framework.Observability.Configuration.Options;
using Framework.Observability.Logging;
using Microsoft.Extensions.Options;

namespace Framework.Observability.Configuration.Validators;

/// <summary>
/// Validates <see cref="SerilogOptions"/> at options bind / host start.
/// </summary>
public sealed class SerilogOptionsValidator : IValidateOptions<SerilogOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, SerilogOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (!LogLevelParser.TryParse(options.MinimumLevel.Default, out _))
        {
            failures.Add(
                $"{SerilogOptions.SectionName}:MinimumLevel:Default is not a valid log level.");
        }

        foreach (var (source, level) in options.MinimumLevel.Override)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                failures.Add($"{SerilogOptions.SectionName}:MinimumLevel:Override keys must be non-empty.");
                continue;
            }

            if (!LogLevelParser.TryParse(level, out _))
            {
                failures.Add(
                    $"{SerilogOptions.SectionName}:MinimumLevel:Override:{source} is not a valid log level.");
            }
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
