namespace CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;

/// <summary>
/// Thresholds for slow-query interceptor severity logging.
/// </summary>
internal sealed class SlowQueryInterceptorOptions
{
    public const string SectionName = "Infrastructure:Persistence:SlowQuery";

    /// <summary>
    /// Durations at or above this value log as Warning (default 250ms).
    /// </summary>
    public TimeSpan WarningThreshold { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Durations at or above this value log as Error (default 500ms).
    /// </summary>
    public TimeSpan ErrorThreshold { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Durations at or above this value log as Critical (default 1s).
    /// </summary>
    public TimeSpan CriticalThreshold { get; set; } = TimeSpan.FromSeconds(1);
}
