using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace CoreQuant.Infrastructure.Persistence.EF;

/// <summary>
/// CoreQuant write DbContext (primary / master). Hosts register this via provider extensions.
/// </summary>
public sealed class CoreQuantWriteDbContext : WriteDbContext
{
    /// <summary>
    /// Creates the CoreQuant write context.
    /// </summary>
    public CoreQuantWriteDbContext(DbContextOptions<CoreQuantWriteDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    protected override Assembly ConfigurationsAssembly => typeof(CoreQuantWriteDbContext).Assembly;
}
