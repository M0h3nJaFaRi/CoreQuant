using Common.Infrastructure.Persistence.EF.Enums;

namespace Common.Infrastructure.Persistence.EF.UnitOfWork;
public interface IUnitOfWorkFactory
{
    IUnitOfWork Create(PersistenceDatabase database);
}