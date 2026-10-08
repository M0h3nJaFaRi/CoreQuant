using Microsoft.EntityFrameworkCore;

namespace CoreQuant.Infrastructure.Persistence.EF;

/// <summary>
/// Read policy layer over <see cref="MainDbContext"/> (NoTracking + mutations forbidden).
/// Prefer <c>AppReadDbContext : ReadDbContext</c>; policies also apply via read-registration interceptors.
/// </summary>
public abstract class ReadDbContext : MainDbContext
{
    /// <summary>
    /// Creates the read context.
    /// </summary>
    protected ReadDbContext(DbContextOptions options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    /// <inheritdoc />
    public override int SaveChanges()
        => throw CreateReadOnlyException();

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
        => throw CreateReadOnlyException();

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw CreateReadOnlyException();

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        => throw CreateReadOnlyException();

    private static InvalidOperationException CreateReadOnlyException()
        => new("Read DbContext is read-only. Use the write DbContext for mutations.");
}
