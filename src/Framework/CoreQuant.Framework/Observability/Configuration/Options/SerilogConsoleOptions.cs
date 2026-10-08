namespace Framework.Observability.Configuration.Options;

/// <summary>
/// Console sink toggle for Serilog.
/// </summary>
public sealed class SerilogConsoleOptions
{
    /// <summary>
    /// When true, writes log events to the console.
    /// </summary>
    public bool Enabled { get; set; }
}
