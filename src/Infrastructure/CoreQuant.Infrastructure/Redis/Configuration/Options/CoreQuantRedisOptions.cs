using StackExchange.Redis;

namespace CoreQuant.Infrastructure.Redis.Configuration.Options;

/// <summary>
/// Connection settings for the shared Redis multiplexer.
/// Bound from <c>Infrastructure:Redis</c>.
/// </summary>
public sealed class CoreQuantRedisOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Infrastructure:Redis";

    /// <summary>
    /// Redis server host.
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// Redis server port.
    /// </summary>
    public int Port { get; set; } = 6379;

    /// <summary>
    /// Optional Redis password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Host/pod identity segment for Redis key prefixes ({Service}:{Host}:...).
    /// </summary>
    public string HostIdentity { get; set; } = Environment.MachineName;

    /// <summary>
    /// When true, fails fast if Redis is unavailable at connect time.
    /// </summary>
    public bool AbortOnConnectFail { get; set; }

    /// <summary>
    /// Connection timeouts and retry settings.
    /// </summary>
    public RedisConnectionOptions Connection { get; set; } = new();

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
