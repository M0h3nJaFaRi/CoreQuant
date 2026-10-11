using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Common.Infrastructure.Persistence.EF.DbContexts;

namespace CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Stocks;

/// <summary>
/// CoreQuant Stocks DataBase EF DbContext. Hosts register this via MySQL extensions.
/// </summary>
public sealed class StockDbContext(DbContextOptions<StockDbContext> options) : BaseDbContext(options)
{
    /// <inheritdoc />
    protected override Assembly ConfigurationsAssembly => typeof(StockDbContext).Assembly;

    protected sealed override void OnModelCreatingInternal(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingInternal(modelBuilder);
    }
}