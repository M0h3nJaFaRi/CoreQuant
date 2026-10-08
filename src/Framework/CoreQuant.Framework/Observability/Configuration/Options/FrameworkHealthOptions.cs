namespace Framework.Observability.Configuration.Options;

/// <summary>
/// HTTP health probe paths exposed by web hosts.
/// </summary>
public sealed class FrameworkHealthOptions
{
    /// <summary>
    /// Configuration section: <c>Framework:Health</c>.
    /// </summary>
    public const string SectionName = "Framework:Health";

    /// <summary>
    /// Liveness probe path.
    /// </summary>
    public string LivenessPath { get; set; } = "/health/live";

    /// <summary>
    /// Readiness probe path.
    /// </summary>
    public string ReadinessPath { get; set; } = "/health/ready";
}
