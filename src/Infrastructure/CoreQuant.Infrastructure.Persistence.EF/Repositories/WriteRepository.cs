using Common.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace CoreQuant.Infrastructure.Persistence.EF.Repositories;

/// <summary>
/// Write-side EF repository helper bound to <see cref="WriteDbContext"/> (primary / master).
/// Read helpers on this type always hit master — never the read replica.
/// </summary>
/// <typeparam name="TAggregate">Aggregate root type.</typeparam>
/// <typeparam name="TId">Aggregate identity type.</typeparam>
internal abstract class WriteRepository<TAggregate, TId> : BaseRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Creates the write repository.
    /// </summary>
    protected WriteRepository(WriteDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <summary>
    /// Typed write context (primary / master).
    /// </summary>
    protected WriteDbContext WriteDbContext => (WriteDbContext)DbContext;

    /// <summary>
    /// Tracking query root against master for <typeparamref name="TAggregate"/>.
    /// </summary>
    protected virtual IQueryable<TAggregate> Query => Set.AsQueryable();

    /// <summary>
    /// Loads an aggregate by id from master with tracking (for subsequent mutation).
    /// </summary>
    protected virtual Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return Query.FirstOrDefaultAsync(aggregate => aggregate.Id.Equals(id), cancellationToken);
    }

    /// <summary>
    /// Loads an aggregate by id from master without tracking (master-consistent read, no mutation).
    /// </summary>
    protected virtual Task<TAggregate?> GetByIdAsNoTrackingAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        return Set.AsNoTracking()
            .FirstOrDefaultAsync(aggregate => aggregate.Id.Equals(id), cancellationToken);
    }

    /// <summary>
    /// Returns whether an aggregate with the given id exists on master.
    /// </summary>
    protected virtual Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        return Query.AnyAsync(aggregate => aggregate.Id.Equals(id), cancellationToken);
    }

    /// <summary>
    /// Adds a new aggregate to the current unit of work (no <c>SaveChanges</c>).
    /// </summary>
    protected virtual Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        cancellationToken.ThrowIfCancellationRequested();

        Set.Add(aggregate);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Soft-deletes an aggregate via domain <see cref="Entity{TId}.MarkAsRemoved"/> on master.
    /// Physical <c>Remove</c> is forbidden.
    /// </summary>
    protected virtual void SoftDelete(TAggregate aggregate, long? deletedBy = null)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        aggregate.MarkAsRemoved(deletedBy);
    }
}
