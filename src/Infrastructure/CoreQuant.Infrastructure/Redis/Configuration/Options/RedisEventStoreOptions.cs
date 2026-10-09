namespace CoreQuant.Infrastructure.Redis.Configuration.Options;

/// <summary>
/// Redis database settings for integration event streams.
/// </summary>
public sealed record RedisEventStoreOptions
{
    /// <summary>
    /// Logical Redis database index for event streams.
    /// </summary>
    public uint Database { get; init; }

    /// <summary>
    /// Approximate max length per stream (Redis MAXLEN ~). Zero disables trimming.
    /// </summary>
    public int MaxStreamLength { get; init; }

    /// <summary>
    /// Default stream name for outbox transport when not overridden per message.
    /// </summary>
    public string DefaultStream { get; init; } = default!;
}
