using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors;

/// <summary>
/// Blocks EF physical deletes; soft delete must be used instead.
/// </summary>
public sealed class RejectPhysicalDeleteInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Reject(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Reject(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Reject(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Deleted)
            {
                throw new InvalidOperationException(
                    $"Physical delete is forbidden for '{entry.Metadata.ClrType.Name}'. Use soft delete instead.");
            }
        }
    }
}
