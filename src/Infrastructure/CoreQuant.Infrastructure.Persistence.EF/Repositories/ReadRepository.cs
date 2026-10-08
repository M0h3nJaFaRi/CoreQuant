using Common.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace CoreQuant.Infrastructure.Persistence.EF.Repositories;

/// <summary>
/// Read-side EF repository helper bound to <see cref="ReadDbContext"/> (replica / NoTracking).
/// </summary>
/// <typeparam name="TAggregate">Aggregate root type.</typeparam>
/// <typeparam name="TId">Aggregate identity type.</typeparam>
internal abstract class ReadRepository<TAggregate, TId> : BaseRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Creates the read repository.
    /// </summary>
    protected ReadRepository(ReadDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <summary>
    /// Typed read context (replica).
    /// </summary>
    protected ReadDbContext ReadDbContext => (ReadDbContext)DbContext;

    /// <summary>
    /// No-tracking query root for <typeparamref name="TAggregate"/>.
    /// </summary>
    protected virtual IQueryable<TAggregate> Query => Set.AsNoTracking();

    /// <summary>
    /// Loads an aggregate by id from the read replica (no tracking).
    /// </summary>
    protected virtual Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return Query.FirstOrDefaultAsync(aggregate => aggregate.Id.Equals(id), cancellationToken);
    }

    /// <summary>
    /// Returns whether an aggregate with the given id exists on the read replica.
    /// </summary>
    protected virtual Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        return Query.AnyAsync(aggregate => aggregate.Id.Equals(id), cancellationToken);
    }
}
