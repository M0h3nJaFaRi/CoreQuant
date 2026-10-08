namespace CoreQuant.Infrastructure.Redis.Configuration.Options;

/// <summary>
/// StackExchange.Redis connection timeouts and retry settings.
/// </summary>
public sealed record RedisConnectionOptions
{
    /// <summary>
    /// Asynchronous operation timeout.
    /// </summary>
    public TimeSpan AsyncTimeout { get; init; }

    /// <summary>
    /// Connection establishment timeout.
    /// </summary>
    public TimeSpan ConnectTimeout { get; init; }

    /// <summary>
    /// Synchronous operation timeout.
    /// </summary>
    public TimeSpan SyncTimeout { get; init; }

    /// <summary>
    /// Number of connect retries.
    /// </summary>
    public int ConnectRetry { get; init; }

    /// <summary>
    /// Keep-alive interval.
    /// </summary>
    public TimeSpan KeepAlive { get; init; }
}