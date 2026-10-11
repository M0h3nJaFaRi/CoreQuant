namespace Common.Domain.Events;

/// <summary>
/// Marker for domain events raised by aggregate roots.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier of this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// UTC time when the event occurred.
    /// </summary>
    DateTimeOffset OccurredOnUtc { get; }

    /// <summary>
    /// Payload schema version for consumers.
    /// </summary>
    uint SchemaVersion { get; }
}