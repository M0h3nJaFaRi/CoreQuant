using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace CoreQuant.Infrastructure.Persistence.EF;

/// <summary>
/// Shared project model: declare all <see cref="DbSet{TEntity}"/> members on this layer (or a subclass).
/// Holds model building only (configurations + soft-delete filters) — not read/write policies.
/// </summary>
/// <remarks>
/// Intended shape:
/// <code>
/// public abstract class AppMainDbContext : MainDbContext
/// {
///     public DbSet&lt;Order&gt; Orders =&gt; Set&lt;Order&gt;();
///     protected override Assembly ConfigurationsAssembly =&gt; typeof(AppMainDbContext).Assembly;
/// }
///
/// // Host write/read types must derive WriteDbContext / ReadDbContext.
/// // Prefer duplicating DbSet declarations or a shared partial/main pattern that still inherits:
/// public sealed class AppWriteDbContext : WriteDbContext { /* DbSets + ConfigurationsAssembly */ }
/// public sealed class AppReadDbContext : ReadDbContext { /* DbSets + ConfigurationsAssembly */ }
/// </code>
/// </remarks>
public abstract class MainDbContext : BaseDbContext
{
    /// <summary>
    /// Creates the main context.
    /// </summary>
    protected MainDbContext(DbContextOptions options)
        : base(options)
    {
    }

    /// <summary>
    /// Assembly that contains <see cref="IEntityTypeConfiguration{TEntity}"/> implementations.
    /// </summary>
    protected abstract Assembly ConfigurationsAssembly { get; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(ConfigurationsAssembly);
        ApplySoftDeleteQueryFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Applies a global query filter so soft-deleted <see cref="Common.Domain.Core.Entity{TId}"/> rows are excluded.
    /// </summary>
    protected static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType is null || entityType.IsOwned())
            {
                continue;
            }

            if (!IsSoftDeletableEntity(entityType.ClrType))
            {
                continue;
            }

            var method = typeof(MainDbContext)
                .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(null, [modelBuilder]);
        }
    }

    private static bool IsSoftDeletableEntity(Type clrType)
    {
        var current = clrType;
        while (current is not null && current != typeof(object))
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Common.Domain.Core.Entity<>))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity =>
            Microsoft.EntityFrameworkCore.EF.Property<DateTimeOffset?>(entity, "DeletedAt") == null);
    }
}
