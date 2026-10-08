namespace CoreQuant.Infrastructure.Redis.Configuration.Options;

/// <summary>
/// Redis database settings for CQRS idempotency entries.
/// </summary>
public sealed record RedisIdempotencyOptions
{
    /// <summary>
    /// Logical Redis database index for idempotency entries.
    /// </summary>
    public uint Database { get; init; }
}