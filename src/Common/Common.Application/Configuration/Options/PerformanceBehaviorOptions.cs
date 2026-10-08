namespace Common.Application.Configuration.Options;

/// <summary>
/// Options for CQRS performance logging behavior.
/// </summary>
internal sealed class PerformanceBehaviorOptions
{
    public const string SectionName = "Cqrs:Performance";

    /// <summary>
    /// Duration above which a warning is logged.
    /// </summary>
    public TimeSpan SlowRequestThreshold { get; set; } = TimeSpan.FromMilliseconds(500);
}
