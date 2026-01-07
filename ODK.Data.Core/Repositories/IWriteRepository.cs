namespace ODK.Data.Core.Repositories;

public interface IWriteRepository<T>
{
    void Add(T entity);
    void AddMany(IEnumerable<T> entities);
    void Delete(T entity);
    void DeleteMany(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateMany(IEnumerable<T> entities);
}
