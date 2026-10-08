namespace Framework.Observability.Configuration.Options;

/// <summary>
/// Serilog minimum level defaults and per-namespace overrides.
/// </summary>
public sealed class SerilogMinimumLevelOptions
{
    /// <summary>
    /// Default minimum level (e.g. Information, Debug, Warning).
    /// </summary>
    public string Default { get; set; } = "Information";

    /// <summary>
    /// Source-context overrides (category → level).
    /// </summary>
    public Dictionary<string, string> Override { get; set; } = new(StringComparer.Ordinal);
}
