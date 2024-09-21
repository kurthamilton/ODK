using ODK.Core;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IReadWriteRepository<T> : IWriteRepository<T> where T : IDatabaseEntity
{
    IDeferredQuerySingle<T> GetById(Guid id);
    IDeferredQuerySingleOrDefault<T> GetByIdOrDefault(Guid id);
    IDeferredQueryMultiple<T> GetByIds(IReadOnlyCollection<Guid> ids);
    void Upsert(T entity);
}
