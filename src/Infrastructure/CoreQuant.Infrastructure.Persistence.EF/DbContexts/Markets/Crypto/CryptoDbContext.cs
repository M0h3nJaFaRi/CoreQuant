using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Common.Infrastructure.Persistence.EF.DbContexts;

namespace CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Crypto;

/// <summary>
/// CoreQuant Crypto DataBase EF DbContext. Hosts register this via MySQL extensions.
/// </summary>
public sealed class CryptoDbContext(DbContextOptions<CryptoDbContext> options) : BaseDbContext(options)
{
    /// <inheritdoc />
    protected override Assembly ConfigurationsAssembly => typeof(CryptoDbContext).Assembly;

    protected sealed override void OnModelCreatingInternal(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingInternal(modelBuilder);
    }
}