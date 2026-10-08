using Common.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace CoreQuant.Infrastructure.Persistence.EF.Repositories;

/// <summary>
/// Shared EF helper for aggregate repositories. Not a Domain <c>IRepository&lt;T&gt;</c> —
/// each aggregate still exposes its own <c>I{Aggregate}Repository</c> contract.
/// </summary>
/// <typeparam name="TAggregate">Aggregate root type.</typeparam>
/// <typeparam name="TId">Aggregate identity type.</typeparam>
internal abstract class BaseRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Creates the repository over the given <see cref="MainDbContext"/> model.
    /// </summary>
    protected BaseRepository(MainDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        DbContext = dbContext;
    }

    /// <summary>
    /// Underlying EF context (read or write specialization).
    /// </summary>
    protected MainDbContext DbContext { get; }

    /// <summary>
    /// Set for <typeparamref name="TAggregate"/>.
    /// </summary>
    protected DbSet<TAggregate> Set => DbContext.Set<TAggregate>();
}
