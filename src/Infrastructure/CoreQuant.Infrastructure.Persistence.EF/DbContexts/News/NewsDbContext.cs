using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Common.Infrastructure.Persistence.EF.DbContexts;

namespace CoreQuant.Infrastructure.Persistence.EF.DbContexts.News;

/// <summary>
/// CoreQuant New DataBase EF DbContext. Hosts register this via MySQL extensions.
/// </summary>
public sealed class NewsDbContext(DbContextOptions<NewsDbContext> options) : BaseDbContext(options)
{
    /// <inheritdoc />
    protected override Assembly ConfigurationsAssembly => typeof(NewsDbContext).Assembly;

    protected sealed override void OnModelCreatingInternal(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingInternal(modelBuilder);
    }
}
