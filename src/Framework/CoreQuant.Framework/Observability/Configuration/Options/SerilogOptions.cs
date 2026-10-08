namespace Framework.Observability.Configuration.Options;

/// <summary>
/// Serilog settings bound from <c>Framework:Serilog</c>.
/// </summary>
public sealed class SerilogOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Framework:Serilog";

    /// <summary>
    /// Minimum level configuration.
    /// </summary>
    public SerilogMinimumLevelOptions MinimumLevel { get; set; } = new();

    /// <summary>
    /// Console sink settings.
    /// </summary>
    public SerilogConsoleOptions Console { get; set; } = new();
}
