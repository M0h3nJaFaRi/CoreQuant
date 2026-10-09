namespace Common.Application.Events;

/// <summary>
/// Integration event payload appended to a Redis event stream (transport only).
/// </summary>
public sealed class RedisEventEnvelope
{
    /// <summary>
    /// Unique event id (outbox / domain message id).
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// Contract type name for consumers.
    /// </summary>
    public required string EventType { get; init; }

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
