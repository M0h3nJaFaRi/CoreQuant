namespace Framework.Observability.Configuration.Options;

/// <summary>
/// Options for Framework observability registration.
/// </summary>
public sealed class FrameworkObservabilityOptions
{
    /// <summary>
    /// Logical service name used for metrics, traces, and health reporting.
    /// </summary>
    public string ServiceName { get; set; } = "service";
}
