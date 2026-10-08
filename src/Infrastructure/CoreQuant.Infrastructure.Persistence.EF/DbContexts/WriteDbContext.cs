using Microsoft.EntityFrameworkCore;

namespace CoreQuant.Infrastructure.Persistence.EF;

/// <summary>
/// Write policy layer over <see cref="MainDbContext"/> (master / primary).
/// Prefer <c>AppWriteDbContext : WriteDbContext</c> and <c>AppReadDbContext : ReadDbContext</c>
/// (shared model members can live on a common abstract main type only if it still derives correctly).
/// </summary>
/// <remarks>
/// Physical-delete protection and soft-delete conversion live in the write registration
/// interceptors (<c>SoftDeleteInterceptor</c> then <c>RejectPhysicalDeleteInterceptor</c>),
/// which providers always wire. A <see cref="DbContext"/> override cannot do this job:
/// it runs before interceptors and would reject soft-deletable entities before
/// <c>SoftDeleteInterceptor</c> has a chance to convert them.
/// </remarks>
public abstract class WriteDbContext : MainDbContext
{
    /// <summary>
    /// Creates the write context.
    /// </summary>
    protected WriteDbContext(DbContextOptions options)
        : base(options)
    {
    }
}
