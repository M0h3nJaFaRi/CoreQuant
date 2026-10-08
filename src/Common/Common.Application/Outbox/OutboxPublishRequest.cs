namespace Common.Application.Outbox;

/// <summary>
/// Request to publish a persisted outbox message to the integration bus.
/// </summary>
public sealed class OutboxPublishRequest
{
    /// <summary>
    /// Outbox / domain event message id.
    /// </summary>
    public required Guid MessageId { get; init; }

    /// <summary>
    /// Contract type name for consumers.
    /// </summary>
    public required string MessageType { get; init; }

    /// <summary>
    /// JSON payload.
    /// </summary>
    public required string Payload { get; init; }

    /// <summary>
    /// UTC time the business event occurred.
    /// </summary>
    public required DateTimeOffset OccurredOnUtc { get; init; }

    /// <summary>
    /// Payload schema version.
    /// </summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>
    /// Optional correlation identifier.
    /// </summary>
    public string? CorrelationId { get; init; }
}
