namespace Common.Application.Outbox;

/// <summary>
/// Envelope for enqueuing a message into the transactional outbox.
/// </summary>
public sealed class OutboxMessageWrite
{
    /// <summary>
    /// Stable message id (typically the domain <c>EventId</c>).
    /// </summary>
    public Guid MessageId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Contract / CLR type name used by consumers to deserialize <see cref="Payload"/>.
    /// </summary>
    public required string MessageType { get; init; }

    /// <summary>
    /// JSON payload produced by <c>Framework.Serialization</c>.
    /// </summary>
    public required string Payload { get; init; }

    /// <summary>
    /// UTC time the business event occurred.
    /// </summary>
    public DateTimeOffset OccurredOnUtc { get; init; }

    /// <summary>
    /// Payload schema version for consumers.
    /// </summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>
    /// Optional correlation identifier from Framework.Observability.
    /// </summary>
    public string? CorrelationId { get; init; }
}
