using Common.Application.Queuing;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.Core;
using Microsoft.Extensions.Options;

namespace CoreQuant.Infrastructure.Redis.Adapters.Queuing;

/// <summary>
/// Redis Streams queue primitive.
/// </summary>
internal sealed class RedisQueue(IRedisCore redis, IOptions<CoreQuantRedisOptions> options) : IRedisQueue
{
    private readonly uint _database = options.Value.Queue.Database;
    private readonly int _maxStreamLength = options.Value.Queue.MaxStreamLength;

    /// <inheritdoc />
    public Task EnsureConsumerGroupAsync(string queueName, string consumerGroup, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerGroup);

        return redis.EnsureConsumerGroupAsync(_database, BuildQueueKey(queueName), consumerGroup, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> EnqueueAsync(string queueName, RedisQueuePayload payload, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentNullException.ThrowIfNull(payload);

        return redis.StreamAddAsync(_database, BuildQueueKey(queueName), payload, _maxStreamLength, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RedisQueueMessage>> DequeueAsync(
        string queueName,
        string consumerGroup,
        string consumerName,
        int count,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerGroup);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerName);

        var records = await redis
            .StreamReadGroupAsync<RedisQueuePayload>(
                _database,
                BuildQueueKey(queueName),
                consumerGroup,
                consumerName,
                count,
                cancellationToken)
            .ConfigureAwait(false);

        if (records.Count == 0)
        {
            return [];
        }

        var messages = new List<RedisQueueMessage>(records.Count);
        foreach (var record in records)
        {
            messages.Add(new RedisQueueMessage
            {
                EntryId = record.EntryId,
                Body = record.Payload
            });
        }

        return messages;
    }

    /// <inheritdoc />
    public Task AckAsync(string queueName, string consumerGroup, string entryId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerGroup);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);

        return redis.StreamAckAsync(_database, BuildQueueKey(queueName), consumerGroup, entryId, cancellationToken);
    }

    private static string BuildQueueKey(string queueName) => $"queue:{queueName.Trim()}";
}
