using Microsoft.EntityFrameworkCore;
using ODK.Data.Core;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework;

public abstract class WriteRepositoryBase<T, TQueryBuilder> : WriteRepositoryBase<T>, IWriteRepository<T, TQueryBuilder>
    where T : class
    where TQueryBuilder : IQueryBuilder<T>
{
    protected WriteRepositoryBase(DbContext context)
        : base(context)
    {
    }

    public abstract TQueryBuilder Query();

    public virtual TQueryBuilder Query(Func<TQueryBuilder, TQueryBuilder> filter)
        => filter(Query());
}

public abstract class WriteRepositoryBase<T> : RepositoryBase, IWriteRepository<T>
    where T : class
{
    protected WriteRepositoryBase(DbContext context)
        : base(context)
    {
    }

    public virtual T Add(T entity)
    {
        AddSingle(entity);
        return entity;
    }

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

    protected TQueryBuilder CreateQueryBuilder<TQueryBuilder>(Func<DbContext, TQueryBuilder> factory)
        where TQueryBuilder : IQueryBuilder<T>
        => CreateQueryBuilder<TQueryBuilder, T>(factory);

    protected virtual IQueryable<T> Set() => Set<T>();
}