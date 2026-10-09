using Common.Application.Configuration.Options;
using Microsoft.Extensions.Options;

namespace Common.Application.Configuration.Validators;

/// <summary>
/// Validates <see cref="OutboxOptions"/> at options bind / host start.
/// </summary>
public sealed class OutboxOptionsValidator : IValidateOptions<OutboxOptions>
{
    private static readonly HashSet<string> AllowedTransports =
        new(StringComparer.OrdinalIgnoreCase) { "Logging", "InMemory", "Redis" };

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, OutboxOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Transport))
        {
            failures.Add($"{OutboxOptions.SectionName}:Transport is required.");
        }
        else if (!AllowedTransports.Contains(options.Transport))
        {
            failures.Add(
                $"{OutboxOptions.SectionName}:Transport must be one of: {string.Join(", ", AllowedTransports)}.");
        }

        if (options.RetryBatchSize <= 0)
        {
            failures.Add($"{OutboxOptions.SectionName}:RetryBatchSize must be greater than zero.");
        }

        if (options.RetryLeaseSeconds <= 0)
        {
            failures.Add($"{OutboxOptions.SectionName}:RetryLeaseSeconds must be greater than zero.");
        }

        if (options.SchedulerIntervalSeconds <= 0)
        {
            failures.Add($"{OutboxOptions.SectionName}:SchedulerIntervalSeconds must be greater than zero.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
