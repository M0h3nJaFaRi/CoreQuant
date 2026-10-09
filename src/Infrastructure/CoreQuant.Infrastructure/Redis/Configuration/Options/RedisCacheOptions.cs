namespace CoreQuant.Infrastructure.Redis.Configuration.Options;

/// <summary>
/// Redis database settings for CQRS query cache entries.
/// </summary>
public sealed record RedisCacheOptions
{
    /// <summary>
    /// Logical Redis database index for cache entries.
    /// </summary>
    public uint Database { get; init; }
}