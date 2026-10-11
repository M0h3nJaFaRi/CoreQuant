namespace Common.Domain.Events;

/// <summary>
/// Base domain event supplying identity, timestamp, and schema version.
/// </summary>
public abstract record DomainEvent(uint schemaVersion = 1) : IDomainEvent
{
    /// <inheritdoc />
    public Guid EventId { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public uint SchemaVersion { get; }
}