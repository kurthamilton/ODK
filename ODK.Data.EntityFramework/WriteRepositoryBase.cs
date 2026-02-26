using Microsoft.EntityFrameworkCore;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework;

public abstract class WriteRepositoryBase<T> : RepositoryBase, IWriteRepository<T>
    where T : class
{
    protected WriteRepositoryBase(DbContext context)
        : base(context)
    {
    }

    public virtual void Add(T entity) => AddSingle(entity);

    public virtual void AddMany(IEnumerable<T> entities) => base.AddMany(entities);

    public virtual void Delete(T entity) => DeleteSingle(entity);

    public virtual void DeleteMany(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            Delete(entity);
        }
    }

    public virtual void Update(T entity) => UpdateSingle(entity);

    public virtual void UpdateMany(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            Update(entity);
        }
    }

    protected virtual IQueryable<T> Set() => Set<T>();
}