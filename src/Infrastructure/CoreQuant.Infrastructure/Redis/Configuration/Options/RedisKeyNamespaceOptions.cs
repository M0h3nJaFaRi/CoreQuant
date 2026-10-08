namespace CoreQuant.Infrastructure.Redis.Configuration.Options;

/// <summary>
/// Resolved service/host segments used to build Redis key prefixes.
/// </summary>
public sealed class RedisKeyNamespaceOptions
{
    /// <summary>
    /// Service name segment (typically <c>ApplicationName</c>).
    /// </summary>
    public string Service { get; set; } = "corequant";

    /// <summary>
    /// Host/pod identity segment.
    /// </summary>
    public string Host { get; set; } = Environment.MachineName;
}
