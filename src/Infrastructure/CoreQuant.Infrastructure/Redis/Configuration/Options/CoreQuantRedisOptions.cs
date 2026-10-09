using StackExchange.Redis;

namespace CoreQuant.Infrastructure.Redis.Configuration.Options;

/// <summary>
/// Connection settings for the shared Redis multiplexer.
/// Bound from <c>Infrastructure:Redis</c>.
/// </summary>
public sealed record CoreQuantRedisOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Infrastructure:Redis";

    /// <summary>
    /// Redis server host.
    /// </summary>
    public string Host { get; init; } = default!;

    /// <summary>
    /// Redis server port.
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Optional Redis password.
    /// </summary>
    public string Password { get; init; } = default!;

    /// <summary>
    /// Host role segment for Redis key prefixes (Api, Worker, Scheduler, Consumer, ...).
    /// </summary>
    public string HostRole { get; init; } = default!;

    /// <summary>
    /// When true, fails fast if Redis is unavailable at connect time.
    /// </summary>
    public bool AbortOnConnectFail { get; init; }

    /// <summary>
    /// Connection timeouts and retry settings.
    /// </summary>
    public RedisConnectionOptions Connection { get; init; } = new();

    /// <summary>
    /// CQRS cache database settings.
    /// </summary>
    public RedisCacheOptions Cache { get; init; } = new();

    /// <summary>
    /// CQRS idempotency database settings.
    /// </summary>
    public RedisIdempotencyOptions Idempotency { get; init; } = new();

    /// <summary>
    /// CQRS rate-limit database settings.
    /// </summary>
    public RedisRateLimitOptions RateLimit { get; init; } = new();

    /// <summary>
    /// Token store database settings.
    /// </summary>
    public RedisTokenOptions Token { get; init; } = new();

    /// <summary>
    /// Distributed lock database settings.
    /// </summary>
    public RedisLockOptions Lock { get; init; } = new();

    /// <summary>
    /// Integration event stream database settings.
    /// </summary>
    public RedisEventStoreOptions EventStore { get; init; } = new();

    /// <summary>
    /// Queue stream database settings.
    /// </summary>
    public RedisQueueOptions Queue { get; init; } = new();

    /// <summary>
    /// Builds StackExchange.Redis <see cref="ConfigurationOptions"/> from these fields.
    /// </summary>
    public ConfigurationOptions ToConfigurationOptions()
    {
        var options = new ConfigurationOptions
        {
            AbortOnConnectFail = AbortOnConnectFail,
            ConnectRetry = Connection.ConnectRetry,
            ConnectTimeout = (int)Connection.ConnectTimeout.TotalMilliseconds,
            SyncTimeout = (int)Connection.SyncTimeout.TotalMilliseconds,
            AsyncTimeout = (int)Connection.AsyncTimeout.TotalMilliseconds,
            KeepAlive = (int)Connection.KeepAlive.TotalSeconds
        };

        options.EndPoints.Add(Host, Port);

        if (!string.IsNullOrWhiteSpace(Password))
        {
            options.Password = Password;
        }

        return options;
    }
}
