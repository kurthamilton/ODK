using ODK.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core;

public interface IReadWriteRepository<T> : IReadWriteRepository<T, IDatabaseEntityQueryBuilder<T>>
    where T : IDatabaseEntity
{
}

public interface IReadWriteRepository<T, TQueryBuilder> : IWriteRepository<T, TQueryBuilder>
    where T : IDatabaseEntity
    where TQueryBuilder : IDatabaseEntityQueryBuilder<T, TQueryBuilder>
{
    IDeferredQuerySingle<T> GetById(Guid id);

    IDeferredQuerySingleOrDefault<T> GetByIdOrDefault(Guid? id);

    IDeferredQueryMultiple<T> GetByIds(IReadOnlyCollection<Guid> ids);

    void Upsert(T entity);
}