using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors;

/// <summary>
/// Ensures a read DbContext never persists changes.
/// </summary>
public sealed class ReadOnlyDbContextInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
        => throw CreateException();

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
        => throw CreateException();

    private static InvalidOperationException CreateException()
        => new("Read DbContext is read-only. Use the write DbContext for mutations.");
}
