namespace Common.Application.Configuration.Options;

/// <summary>
/// Configuration for outbox publish transport and retry job.
/// </summary>
public sealed class OutboxOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Application:Outbox";

    /// <summary>
    /// Publish transport: <c>Logging</c> (local/dev) or <c>InMemory</c> (process-local broker stand-in).
    /// </summary>
    public string Transport { get; set; } = "Logging";

    /// <summary>
    /// Max pending messages processed per retry job run.
    /// </summary>
    public int RetryBatchSize { get; set; } = 50;

    /// <summary>
    /// Exclusive lease duration for a retry run.
    /// </summary>
    public int RetryLeaseSeconds { get; set; } = 30;

    /// <summary>
    /// Scheduler interval between retry job invocations.
    /// </summary>
    public int SchedulerIntervalSeconds { get; set; } = 15;
}
