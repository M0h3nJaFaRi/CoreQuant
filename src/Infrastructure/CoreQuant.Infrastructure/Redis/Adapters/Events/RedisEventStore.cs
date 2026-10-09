using Common.Application.Events;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.Core;
using Microsoft.Extensions.Options;

namespace CoreQuant.Infrastructure.Redis.Adapters.Events;

/// <summary>
/// Redis Streams transport for integration events.
/// </summary>
internal sealed class RedisEventStore(IRedisCore redis, IOptions<CoreQuantRedisOptions> options) : IRedisEventStore
{
    private readonly uint _database = options.Value.EventStore.Database;
    private readonly int _maxStreamLength = options.Value.EventStore.MaxStreamLength;

    /// <inheritdoc />
    public Task EnsureConsumerGroupAsync(string stream, string consumerGroup, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerGroup);

        return redis.EnsureConsumerGroupAsync(_database, BuildStreamKey(stream), consumerGroup, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> AppendAsync(string stream, RedisEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stream);
        ArgumentNullException.ThrowIfNull(envelope);

        return redis.StreamAddAsync(_database, BuildStreamKey(stream), envelope, _maxStreamLength, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RedisEventStoreMessage>> ReadAsync(
        string stream,
        string consumerGroup,
        string consumerName,
        int count,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerGroup);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerName);

        var records = await redis
            .StreamReadGroupAsync<RedisEventEnvelope>(
                _database,
                BuildStreamKey(stream),
                consumerGroup,
                consumerName,
                count,
                cancellationToken)
            .ConfigureAwait(false);

        if (records.Count == 0)
        {
            return [];
        }

        var messages = new List<RedisEventStoreMessage>(records.Count);
        foreach (var record in records)
        {
            messages.Add(new RedisEventStoreMessage
            {
                EntryId = record.EntryId,
                Envelope = record.Payload
            });
        }

        return messages;
    }

    /// <inheritdoc />
    public Task AckAsync(string stream, string consumerGroup, string entryId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerGroup);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);

        return redis.StreamAckAsync(_database, BuildStreamKey(stream), consumerGroup, entryId, cancellationToken);
    }

    private static string BuildStreamKey(string stream) => $"event:{stream.Trim()}";
}
