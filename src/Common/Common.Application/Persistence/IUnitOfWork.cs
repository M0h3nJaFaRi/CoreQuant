namespace Common.Application.Persistence;

/// <summary>
/// Coordinates the persistence transaction boundary for a use case.
/// Implemented by Infrastructure (e.g. EF Unit of Work).
/// </summary>
public interface IUnitOfWork
{
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
}
