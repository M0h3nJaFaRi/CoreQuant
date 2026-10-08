namespace Framework.Infrastructure.Configuration.Options;

/// <summary>
/// StackExchange.Redis connection timeouts and retry settings.
/// </summary>
public sealed class RedisConnectionOptions
{
    /// <summary>
    /// Asynchronous operation timeout.
    /// </summary>
    public TimeSpan AsyncTimeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Connection establishment timeout.
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Synchronous operation timeout.
    /// </summary>
    public TimeSpan SyncTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Number of connect retries.
    /// </summary>
    public int ConnectRetry { get; set; } = 3;

    /// <summary>
    /// Keep-alive interval.
    /// </summary>
    public TimeSpan KeepAlive { get; set; } = TimeSpan.FromMinutes(1);
}
