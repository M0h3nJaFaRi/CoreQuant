namespace Common.Application.Events;

/// <summary>
/// A single message read from a Redis event stream.
/// </summary>
public sealed class RedisEventStoreMessage
{
    /// <summary>
    /// Redis stream entry id.
    /// </summary>
    public required string EntryId { get; init; }

    /// <summary>
    /// Deserialized event envelope.
    /// </summary>
    public required RedisEventEnvelope Envelope { get; init; }
}
