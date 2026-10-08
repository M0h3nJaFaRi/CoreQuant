namespace Framework.Observability.Configuration.Options;

/// <summary>
/// HTTP metrics scrape endpoint options.
/// </summary>
public sealed class FrameworkMetricsOptions
{
    /// <summary>
    /// Configuration section: <c>Framework:Metrics</c>.
    /// </summary>
    public const string SectionName = "Framework:Metrics";

    /// <summary>
    /// When false, the metrics endpoint is not mapped.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Metrics scrape path.
    /// </summary>
    public string Path { get; set; } = "/metrics";
}
