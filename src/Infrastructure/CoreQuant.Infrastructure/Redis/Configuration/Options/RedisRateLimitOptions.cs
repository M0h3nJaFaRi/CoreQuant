namespace CoreQuant.Infrastructure.Redis.Configuration.Options;

/// <summary>
/// Redis database settings for CQRS rate-limit windows.
/// </summary>
public sealed record RedisRateLimitOptions
{
    /// <summary>
    /// Logical Redis database index for rate-limit windows.
    /// </summary>
    public uint Database { get; init; }
}