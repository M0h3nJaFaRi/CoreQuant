using Microsoft.Extensions.DependencyInjection;
using Common.Infrastructure.Persistence.EF.UnitOfWork;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Core;
using Common.Infrastructure.Persistence.EF.Enums;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.News;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Stocks;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Metals;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Indices;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Forex;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Crypto;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Commodity;

namespace CoreQuant.Infrastructure.Persistence.EF.UnitOfWorks;

internal sealed class UnitOfWorkFactory(IServiceProvider serviceProvider) : IUnitOfWorkFactory
{
    public IUnitOfWork Create(PersistenceDatabase database)
    {
        return database switch
        {
            PersistenceDatabase.Core => serviceProvider.GetRequiredService<EfUnitOfWork<CoreQuantDbContext>>(),
            PersistenceDatabase.News => serviceProvider.GetRequiredService<EfUnitOfWork<NewsDbContext>>(),
            PersistenceDatabase.Forex => serviceProvider.GetRequiredService<EfUnitOfWork<ForexDbContext>>(),
            PersistenceDatabase.Crypto => serviceProvider.GetRequiredService<EfUnitOfWork<CryptoDbContext>>(),
            PersistenceDatabase.Stocks => serviceProvider.GetRequiredService<EfUnitOfWork<StockDbContext>>(),
            PersistenceDatabase.Metals => serviceProvider.GetRequiredService<EfUnitOfWork<MetalDbContext>>(),
            PersistenceDatabase.Commodity => serviceProvider.GetRequiredService<EfUnitOfWork<CommodityDbContext>>(),
            PersistenceDatabase.Indices => serviceProvider.GetRequiredService<EfUnitOfWork<IndexDbContext>>(),

            _ => throw new ArgumentOutOfRangeException(nameof(database), database, null)
        };
    }
}