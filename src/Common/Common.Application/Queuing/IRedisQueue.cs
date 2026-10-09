namespace Common.Application.Queuing;

/// <summary>
/// Redis Streams queue primitive (no job manager semantics).
/// </summary>
public interface IRedisQueue
{
    /// <summary>
    /// Ensures a consumer group exists for <paramref name="queueName"/>.
    /// </summary>
    Task EnsureConsumerGroupAsync(
        string queueName,
        string consumerGroup,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enqueues a message and returns the Redis entry id.
    /// </summary>
    Task<string> EnqueueAsync(
        string queueName,
        RedisQueuePayload payload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads up to <paramref name="count"/> new messages for a consumer group member.
    /// </summary>
    Task<IReadOnlyList<RedisQueueMessage>> DequeueAsync(
        string queueName,
        string consumerGroup,
        string consumerName,
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges successful processing of a queue entry.
    /// </summary>
    Task AckAsync(
        string queueName,
        string consumerGroup,
        string entryId,
        CancellationToken cancellationToken = default);
}
