namespace Common.Domain.Events;

/// <summary>
/// Base domain event supplying identity, timestamp, and schema version.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    /// Initializes event metadata.
    /// </summary>
    protected DomainEvent(int schemaVersion = 1)
    {
        EventId = Guid.NewGuid();
        OccurredOnUtc = DateTimeOffset.UtcNow;
        SchemaVersion = schemaVersion;
    }

    /// <inheritdoc />
    public Guid EventId { get; init; }

    /// <inheritdoc />
    public DateTimeOffset OccurredOnUtc { get; init; }

    /// <inheritdoc />
    public int SchemaVersion { get; init; }
}
