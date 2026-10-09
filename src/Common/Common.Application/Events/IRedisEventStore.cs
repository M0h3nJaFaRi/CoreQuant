namespace Common.Application.Events;

/// <summary>
/// Redis Streams transport for integration events (not the system of record).
/// </summary>
public interface IRedisEventStore
{
    /// <summary>
    /// Ensures a consumer group exists for <paramref name="stream"/>.
    /// </summary>
    Task EnsureConsumerGroupAsync(
        string stream,
        string consumerGroup,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends an event to the stream and returns the Redis entry id.
    /// </summary>
    Task<string> AppendAsync(
        string stream,
        RedisEventEnvelope envelope,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads new messages for a consumer group member.
    /// </summary>
    Task<IReadOnlyList<RedisEventStoreMessage>> ReadAsync(
        string stream,
        string consumerGroup,
        string consumerName,
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges successful processing of a stream entry.
    /// </summary>
    Task AckAsync(
        string stream,
        string consumerGroup,
        string entryId,
        CancellationToken cancellationToken = default);
}
