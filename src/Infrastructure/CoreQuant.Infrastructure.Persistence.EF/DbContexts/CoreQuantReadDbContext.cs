using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace CoreQuant.Infrastructure.Persistence.EF;

/// <summary>
/// CoreQuant read DbContext (replica). Hosts register this via provider extensions.
/// </summary>
public sealed class CoreQuantReadDbContext : ReadDbContext
{
    /// <summary>
    /// Creates the CoreQuant read context.
    /// </summary>
    public CoreQuantReadDbContext(DbContextOptions<CoreQuantReadDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    protected override Assembly ConfigurationsAssembly => typeof(CoreQuantReadDbContext).Assembly;
}
