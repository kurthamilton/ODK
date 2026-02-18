using ODK.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IReadWriteRepository<T> : IReadWriteRepository<T, IDatabaseEntityQueryBuilder<T>>
    where T : IDatabaseEntity
{
}

public interface IReadWriteRepository<T, TBuilder> : IWriteRepository<T>
    where T : IDatabaseEntity
    where TBuilder : IDatabaseEntityQueryBuilder<T, TBuilder>
{
    IDeferredQuerySingle<T> GetById(Guid id);
    IDeferredQuerySingleOrDefault<T> GetByIdOrDefault(Guid id);
    IDeferredQueryMultiple<T> GetByIds(IReadOnlyCollection<Guid> ids);
    TBuilder Query();
    void Upsert(T entity);
}
