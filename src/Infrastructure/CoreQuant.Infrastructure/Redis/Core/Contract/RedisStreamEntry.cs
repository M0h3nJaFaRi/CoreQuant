namespace CoreQuant.Infrastructure.Redis.Core.Contract;

/// <summary>
/// One deserialized entry read from a Redis stream.
/// </summary>
internal sealed class RedisStreamEntry<T>
{
    /// <summary>
    /// Stream entry id.
    /// </summary>
    public required string EntryId { get; init; }

    /// <summary>
    /// Deserialized payload.
    /// </summary>
    public required T Payload { get; init; }
}
