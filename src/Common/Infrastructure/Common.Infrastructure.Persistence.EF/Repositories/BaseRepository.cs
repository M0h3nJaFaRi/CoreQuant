using Common.Domain.Core;
using Common.Infrastructure.Persistence.EF.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence.EF.Repositories;

/// <summary>
/// Shared EF helper for aggregate repositories. Not a Domain <c>IRepository&lt;T&gt;</c> —
/// each aggregate still exposes its own <c>I{Aggregate}Repository</c> contract.
/// </summary>
/// <typeparam name="TDbContext">DbContext type.</typeparam>
/// <typeparam name="TAggregate">Aggregate root type.</typeparam>
/// <typeparam name="TId">Aggregate identity type.</typeparam>
internal abstract class BaseRepository<TDbContext, TAggregate, TId>(TDbContext dbContext)
where TAggregate : AggregateRoot<TId>
where TDbContext : BaseDbContext
where TId : notnull
{
    /// <summary>
    /// Underlying EF context (read or write specialization).
    /// </summary>
    protected TDbContext DbContext { get; } = dbContext;

    /// <summary>
    /// Set for <typeparamref name="TAggregate"/>.
    /// </summary>
    protected DbSet<TAggregate> Set => DbContext.Set<TAggregate>();

    /// <summary>
    /// Tracking query root for <typeparamref name="TAggregate"/>.
    /// </summary>
    protected virtual IQueryable<TAggregate> Query => Set.AsQueryable();

    /// <summary>
    /// No-tracking query root for read projections and queries.
    /// </summary>
    protected virtual IQueryable<TAggregate> ReadQuery => Set.AsNoTracking();

    /// <summary>
    /// Loads an aggregate by id with tracking (for subsequent mutation).
    /// </summary>
    protected virtual Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return Query.FirstOrDefaultAsync(aggregate => aggregate.Id.Equals(id), cancellationToken);
    }

    /// <summary>
    /// Loads an aggregate by id without tracking (read-only, no mutation).
    /// </summary>
    protected virtual Task<TAggregate?> GetByIdAsNoTrackingAsync(TId id, CancellationToken cancellationToken = default)
    {
        return ReadQuery.FirstOrDefaultAsync(aggregate => aggregate.Id.Equals(id), cancellationToken);
    }

    /// <summary>
    /// Returns whether an aggregate with the given id exists.
    /// </summary>
    protected virtual Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        return ReadQuery.AnyAsync(aggregate => aggregate.Id.Equals(id), cancellationToken);
    }

    /// <summary>
    /// Adds a new aggregate to the current unit of work (no <c>SaveChanges</c>).
    /// </summary>
    protected virtual async Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        
        await Set.AddAsync(aggregate, cancellationToken);
    }

    /// <summary>
    /// Soft-deletes an aggregate via domain <see cref="Entity{TId}.MarkAsRemoved"/>.
    /// Physical <c>Remove</c> is forbidden.
    /// </summary>
    protected virtual void SoftDelete(TAggregate aggregate, ulong? deletedBy = null)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        aggregate.MarkAsRemoved(deletedBy);
    }
}