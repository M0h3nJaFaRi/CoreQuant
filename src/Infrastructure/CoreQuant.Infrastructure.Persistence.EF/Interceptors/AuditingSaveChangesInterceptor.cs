using Common.Application.Authorization;
using Common.Domain.Core;
using Framework.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors;

/// <summary>
/// Stamps row audit actors/timestamps on save without double-incrementing <see cref="Entity{TId}.Version"/>.
/// Domain <c>Touch</c>/<c>MarkAsRemoved</c> remain responsible for version bumps.
/// </summary>
internal sealed class AuditingSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ISystemClock _clock;

    public AuditingSaveChangesInterceptor(ICurrentUserAccessor currentUser, ISystemClock clock)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(clock);
        _currentUser = currentUser;
        _clock = clock;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var userId = _currentUser.UserId;
        var now = _clock.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (!IsDomainEntity(entry.Entity))
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    StampAdded(entry, userId, now);
                    break;
                case EntityState.Modified:
                    StampModified(entry, userId, now);
                    break;
            }
        }
    }

    private static void StampAdded(EntityEntry entry, long? userId, DateTimeOffset now)
    {
        var createdAt = entry.Property(nameof(Entity<long>.CreatedAt));
        if (createdAt.CurrentValue is DateTimeOffset created && created == default)
        {
            createdAt.CurrentValue = now;
        }

        var createdBy = entry.Property(nameof(Entity<long>.CreatedBy));
        if (createdBy.CurrentValue is null && userId is not null)
        {
            createdBy.CurrentValue = userId;
        }
    }

    private static void StampModified(EntityEntry entry, long? userId, DateTimeOffset now)
    {
        // Soft-delete already stamped Deleted*/Modified* via MarkAsRemoved; still refresh ModifiedAt from clock.
        entry.Property(nameof(Entity<long>.ModifiedAt)).CurrentValue = now;
        entry.Property(nameof(Entity<long>.ModifiedAt)).IsModified = true;

        if (userId is not null)
        {
            entry.Property(nameof(Entity<long>.ModifiedBy)).CurrentValue = userId;
            entry.Property(nameof(Entity<long>.ModifiedBy)).IsModified = true;
        }
    }

    private static bool IsDomainEntity(object entity)
    {
        for (var type = entity.GetType(); type is not null; type = type.BaseType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Entity<>))
            {
                return true;
            }
        }

        return false;
    }
}
