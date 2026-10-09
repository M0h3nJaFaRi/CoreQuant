namespace Common.Application.Queuing;

/// <summary>
/// A message dequeued from a Redis queue stream.
/// </summary>
public sealed class RedisQueueMessage
{
    /// <summary>
    /// Redis stream entry id.
    /// </summary>
    public required string EntryId { get; init; }

    /// <summary>
    /// Deserialized payload.
    /// </summary>
    public required RedisQueuePayload Body { get; init; }
}
