namespace CoreQuant.Infrastructure.Redis.Configuration.Options;

/// <summary>
/// Redis database settings for distributed tokens.
/// </summary>
public sealed record RedisTokenOptions
{
    /// <summary>
    /// Logical Redis database index for token entries.
    /// </summary>
    public uint Database { get; init; }
}