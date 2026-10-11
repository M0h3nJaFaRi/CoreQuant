using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Common.Infrastructure.Persistence.EF.DbContexts;

/// <summary>
/// Generic EF Core root. No persistence policies — only a typed entry point over <see cref="DbContext"/>.
/// </summary>
public abstract class BaseDbContext(DbContextOptions options) : DbContext(options)
{
    /// <summary>
    /// Assembly that contains <see cref="IEntityTypeConfiguration{TEntity}"/> implementations.
    /// </summary>
    protected abstract Assembly ConfigurationsAssembly { get; }

    /// <inheritdoc />
    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(ConfigurationsAssembly);

        OnModelCreatingInternal(modelBuilder);

        ApplySoftDeleteQueryFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Applies a global filter.
    /// </summary>
    protected virtual void OnModelCreatingInternal(ModelBuilder modelBuilder) { }


    #region Apply Soft Delete

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType is null || entityType.IsOwned())
                continue;

            if (!IsSoftDeletableEntity(entityType.ClrType))
                continue;

            var method = typeof(BaseDbContext).GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(entityType.ClrType);

            method.Invoke(null, [modelBuilder]);
        }
    }

    private static bool IsSoftDeletableEntity(Type clrType)
    {
        var current = clrType;

        while (current is not null && current != typeof(object))
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Common.Domain.Core.Entity<>))
                return true;

            current = current.BaseType;
        }

        return false;
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
    where TEntity : class
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity => Microsoft.EntityFrameworkCore.EF.Property<DateTimeOffset?>(entity, "DeletedAt") == null);
    } 

    #endregion
}