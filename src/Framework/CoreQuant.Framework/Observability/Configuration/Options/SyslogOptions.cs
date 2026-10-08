namespace Framework.Observability.Configuration.Options;

/// <summary>
/// Syslog sink settings bound from <c>Framework:Syslog</c>.
/// </summary>
public sealed class SyslogOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Framework:Syslog";

    /// <summary>
    /// When true, forwards log events to the configured syslog endpoint.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Syslog server host or IP.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Syslog UDP port (typically 514).
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Message format: <c>RFC5424</c> or <c>RFC3164</c>.
    /// </summary>
    public string Format { get; set; } = "RFC5424";

    /// <summary>
    /// Minimum level accepted by the syslog sink.
    /// </summary>
    public string RestrictedToMinimumLevel { get; set; } = "Information";
}
