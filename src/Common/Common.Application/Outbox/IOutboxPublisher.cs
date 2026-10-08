namespace Common.Application.Outbox;

/// <summary>
/// Publishes outbox messages to the integration transport after the write transaction has committed.
/// </summary>
public interface IOutboxPublisher
{
    /// <summary>
    /// Publishes a single outbox message. Must be idempotent at the consumer when retried (at-least-once).
    /// </summary>
    Task PublishAsync(OutboxPublishRequest request, CancellationToken cancellationToken = default);
}
