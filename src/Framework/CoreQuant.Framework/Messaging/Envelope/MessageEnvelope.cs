namespace Framework.Messaging.Envelope;

/// <summary>
/// Transport-agnostic wrapper for a message payload. Broker-specific envelopes do not belong here.
/// </summary>
/// <typeparam name="TPayload">Payload type carried by the message.</typeparam>
public sealed class MessageEnvelope<TPayload>
{
    /// <summary>
    /// Creates a message envelope.
    /// </summary>
    public MessageEnvelope(
        Guid messageId,
        string messageType,
        DateTimeOffset occurredOnUtc,
        TPayload payload,
        string? correlationId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);
        ArgumentNullException.ThrowIfNull(payload);

        MessageId = messageId == Guid.Empty ? Guid.NewGuid() : messageId;
        MessageType = messageType;
        OccurredOnUtc = occurredOnUtc;
        Payload = payload;
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Unique identifier of this message instance.
    /// </summary>
    public Guid MessageId { get; }

    /// <summary>
    /// Logical message type name (not a .NET assembly-qualified name).
    /// </summary>
    public string MessageType { get; }

    /// <summary>
    /// UTC timestamp when the message was produced.
    /// </summary>
    public DateTimeOffset OccurredOnUtc { get; }

    /// <summary>
    /// Optional correlation identifier linking related work.
    /// </summary>
    public string? CorrelationId { get; }

    /// <summary>
    /// Message body.
    /// </summary>
    public TPayload Payload { get; }

    /// <summary>
    /// Creates an envelope with a new message id and the current UTC time from <paramref name="occurredOnUtc"/>.
    /// </summary>
    public static MessageEnvelope<TPayload> Create(
        string messageType,
        TPayload payload,
        DateTimeOffset occurredOnUtc,
        string? correlationId = null)
        => new(Guid.NewGuid(), messageType, occurredOnUtc, payload, correlationId);
}
