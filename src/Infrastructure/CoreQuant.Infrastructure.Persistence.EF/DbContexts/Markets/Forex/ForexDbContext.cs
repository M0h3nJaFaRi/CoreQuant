using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Common.Infrastructure.Persistence.EF.DbContexts;

namespace CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Forex;

/// <summary>
/// CoreQuant Forex DataBase EF DbContext. Hosts register this via MySQL extensions.
/// </summary>
public sealed class ForexDbContext(DbContextOptions<ForexDbContext> options) : BaseDbContext(options)
{
    /// <inheritdoc />
    protected override Assembly ConfigurationsAssembly => typeof(ForexDbContext).Assembly;

    protected sealed override void OnModelCreatingInternal(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingInternal(modelBuilder);
    }
}