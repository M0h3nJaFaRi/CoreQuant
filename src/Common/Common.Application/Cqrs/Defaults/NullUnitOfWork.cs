using Common.Application.Persistence;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// No-op unit of work for hosts/tests without a persistence provider.
/// </summary>
internal sealed class NullUnitOfWork : IUnitOfWork
{
    /// <inheritdoc />
    public Task BeginAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
