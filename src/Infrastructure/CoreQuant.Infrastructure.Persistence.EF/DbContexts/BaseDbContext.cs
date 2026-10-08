using Microsoft.EntityFrameworkCore;

namespace CoreQuant.Infrastructure.Persistence.EF;

/// <summary>
/// Generic EF Core root. No persistence policies — only a typed entry point over <see cref="DbContext"/>.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    /// <summary>
    /// Creates the context.
    /// </summary>
    protected BaseDbContext(DbContextOptions options)
        : base(options)
    {
    }
}
