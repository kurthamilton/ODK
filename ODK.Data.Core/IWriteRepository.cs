using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core;

public interface IWriteRepository<T, TQueryBuilder> : IWriteRepository<T>
{
    TQueryBuilder Query();
    TQueryBuilder Query(Func<TQueryBuilder, TQueryBuilder> filter);
}

public interface IWriteRepository<T>
{
    T Add(T entity);
    void AddMany(IEnumerable<T> entities);
    void Delete(T entity);
    void DeleteMany(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateMany(IEnumerable<T> entities);
}
