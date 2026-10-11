using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Common.Infrastructure.Persistence.EF.DbContexts;

namespace CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Commodity;

/// <summary>
/// CoreQuant Commodity DataBase EF DbContext. Hosts register this via MySQL extensions.
/// </summary>
public sealed class CommodityDbContext(DbContextOptions<CommodityDbContext> options) : BaseDbContext(options)
{
    /// <inheritdoc />
    protected override Assembly ConfigurationsAssembly => typeof(CommodityDbContext).Assembly;

    protected sealed override void OnModelCreatingInternal(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingInternal(modelBuilder);
    }
}