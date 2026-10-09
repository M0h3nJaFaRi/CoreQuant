namespace CoreQuant.Infrastructure.Redis.Configuration.Options;

/// <summary>
/// Redis database settings for queue streams.
/// </summary>
public sealed record RedisQueueOptions
{
    /// <summary>
    /// Logical Redis database index for queue streams.
    /// </summary>
    public uint Database { get; init; }

    /// <summary>
    /// Approximate max length per queue stream (Redis MAXLEN ~). Zero disables trimming.
    /// </summary>
    public int MaxStreamLength { get; init; }
}