using Common.Application.Authorization;
using Common.Domain.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors;

/// <summary>
/// Converts physical <see cref="EntityState.Deleted"/> into soft delete via <see cref="Entity{TId}.MarkAsRemoved"/>.
/// </summary>
internal sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserAccessor _currentUser;

    public SoftDeleteInterceptor(ICurrentUserAccessor currentUser)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConvertDeletes(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertDeletes(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ConvertDeletes(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var userId = _currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            if (!TryMarkAsRemoved(entry.Entity, userId))
            {
                continue;
            }

            entry.State = EntityState.Modified;
            MarkSoftDeleteProperties(entry);
        }
    }

    private static bool TryMarkAsRemoved(object entity, long? userId)
    {
        var entityType = FindEntityBaseType(entity.GetType());
        if (entityType is null)
        {
            return false;
        }

        var method = entityType.GetMethod(
            nameof(Entity<long>.MarkAsRemoved),
            [typeof(long?)]);

        if (method is null)
        {
            return false;
        }

        method.Invoke(entity, [userId]);
        return true;
    }

    private static Type? FindEntityBaseType(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Entity<>))
            {
                return current;
            }
        }

        return null;
    }

    private static void MarkSoftDeleteProperties(EntityEntry entry)
    {
        SetModifiedIfExists(entry, nameof(Entity<long>.DeletedAt));
        SetModifiedIfExists(entry, nameof(Entity<long>.DeletedBy));
        SetModifiedIfExists(entry, nameof(Entity<long>.ModifiedAt));
        SetModifiedIfExists(entry, nameof(Entity<long>.ModifiedBy));
        SetModifiedIfExists(entry, nameof(Entity<long>.Version));
    }

    private static void SetModifiedIfExists(EntityEntry entry, string propertyName)
    {
        if (entry.Metadata.FindProperty(propertyName) is not null)
        {
            entry.Property(propertyName).IsModified = true;
        }
    }
}
