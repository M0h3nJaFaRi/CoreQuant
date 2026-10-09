namespace Common.Application.Queuing;

/// <summary>
/// Opaque queue message body (infrastructure primitive; job manager will consume later).
/// </summary>
public sealed class RedisQueuePayload
{
    /// <summary>
    /// Logical message type name.
    /// </summary>
    public required string MessageType { get; init; }

    /// <summary>
    /// JSON payload.
    /// </summary>
    public required string Payload { get; init; }

    /// <summary>
    /// Optional correlation identifier.
    /// </summary>
    public string? CorrelationId { get; init; }
}
