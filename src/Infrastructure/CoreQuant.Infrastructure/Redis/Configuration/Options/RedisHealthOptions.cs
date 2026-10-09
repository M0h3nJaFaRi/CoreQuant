namespace CoreQuant.Infrastructure.Redis.Configuration.Options;

/// <summary>
/// Redis readiness probe settings.
/// </summary>
public sealed class RedisHealthOptions
{
    /// <summary>
    /// When false, the Redis readiness check is not registered.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Maximum time to wait for <c>PING</c>.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Logical database index used for the health ping.
    /// </summary>
    public uint? PingDatabase { get; init; }
}
