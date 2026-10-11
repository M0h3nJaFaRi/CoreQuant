namespace Common.Infrastructure.Persistence.EF.UnitOfWork;
/// <summary>
/// Coordinates the persistence transaction boundary for a use case.
/// Implemented by Infrastructure (e.g. EF Unit of Work).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Save changes with out transaction
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a transactional scope when supported by the implementation.
    /// </summary>
    Task BeginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes and commits the transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction when one is active.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a database transaction, runs <paramref name="action"/>, then commits.
    /// Does not call SaveChanges — the action (typically the command handler) must persist explicitly.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the EF ChangeTracker so failed in-memory mutations are not flushed by a later SaveChanges.
    /// Does not roll back the database transaction.
    /// </summary>
    void ClearTrackedChanges();
}