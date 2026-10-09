using Common.Application.Events;
using Common.Application.Outbox;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using Microsoft.Extensions.Options;

namespace CoreQuant.Infrastructure.Redis.Adapters.Events;

/// <summary>
/// Publishes committed outbox messages to the Redis event store transport.
/// </summary>
public sealed class RedisEventStoreOutboxPublisher(
    IRedisEventStore eventStore,
    IOptions<CoreQuantRedisOptions> redisOptions) : IOutboxPublisher
{
    /// <inheritdoc />
    public Task PublishAsync(OutboxPublishRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var stream = redisOptions.Value.EventStore.DefaultStream;
        var envelope = new RedisEventEnvelope
        {
            EventId = request.MessageId,
            EventType = request.MessageType,
            Payload = request.Payload,
            OccurredOnUtc = request.OccurredOnUtc,
            SchemaVersion = request.SchemaVersion,
            CorrelationId = request.CorrelationId
        };

        return eventStore.AppendAsync(stream, envelope, cancellationToken);
    }
}
