namespace CoreQuant.Infrastructure.Redis.Configuration.Options;

/// <summary>
/// Redis database settings for distributed locks.
/// </summary>
public sealed record RedisLockOptions
{
    /// <summary>
    /// Logical Redis database index for lock entries.
    /// </summary>
    public uint Database { get; init; }
}