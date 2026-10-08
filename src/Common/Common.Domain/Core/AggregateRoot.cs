using Common.Domain.Events;

namespace Common.Domain.Core;

/// <summary>
/// Aggregate root that owns domain events for itself and its internal entities.
/// </summary>
/// <typeparam name="TId">Identifier type.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// EF Core materialization constructor.
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Creates an aggregate with the given identity.
    /// </summary>
    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    /// <summary>
    /// Raised domain events awaiting dispatch.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event on this aggregate.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears pending domain events after they have been dispatched.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
