using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Core;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Commodity;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Crypto;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Forex;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Indices;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Metals;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Stocks;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.News;
using Common.Infrastructure.Persistence.EF.DbContexts;

namespace CoreQuant.Infrastructure.Persistence.EF.Configuration;

/// <summary>
/// Maps each CoreQuant EF context to its <see cref="Configuration.Options.DatabaseOptions"/> key.
/// </summary>
public static class CoreQuantDbContextRegistration
{
    public static readonly IReadOnlyList<DbContextRegistration> All =
    [
        new(typeof(NewsDbContext), "News"),
        new(typeof(ForexDbContext), "Forex"),
        new(typeof(IndexDbContext), "Index"),
        new(typeof(MetalDbContext), "Metal"),
        new(typeof(StockDbContext), "Stock"),
        new(typeof(CryptoDbContext), "Crypto"),
        new(typeof(CoreQuantDbContext), "Core"),
        new(typeof(CommodityDbContext), "Commodity"),
    ];

    public sealed record DbContextRegistration(Type ContextType, string ConfigurationKey);
}
