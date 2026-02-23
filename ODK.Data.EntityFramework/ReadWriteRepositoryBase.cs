using ODK.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework;

public abstract class ReadWriteRepositoryBase<T> : ReadWriteRepositoryBase<T, IDatabaseEntityQueryBuilder<T>>
    where T : class, IDatabaseEntity
{
    protected ReadWriteRepositoryBase(OdkContext context) 
        : base(context)
    {
    }

    public override IDeferredQuerySingle<T> GetById(Guid id)
        => Set()
            .Where(x => x.Id == id)
            .DeferredSingle();

    public override IDeferredQuerySingleOrDefault<T> GetByIdOrDefault(Guid id)
        => Set()
            .Where(x => x.Id == id)
            .DeferredSingleOrDefault();

    public override IDeferredQueryMultiple<T> GetByIds(IReadOnlyCollection<Guid> ids)
        => Set()
            .Where(x => ids.Contains(x.Id))
            .DeferredMultiple();

    public override IDatabaseEntityQueryBuilder<T> Query() 
        => CreateQueryBuilder<IDatabaseEntityQueryBuilder<T>, T>(
            context => new DatabaseEntityQueryBuilder<T>(context));
}

public abstract class ReadWriteRepositoryBase<T, TBuilder> : WriteRepositoryBase<T>, IReadWriteRepository<T, TBuilder> 
    where T : class, IDatabaseEntity
    where TBuilder : IDatabaseEntityQueryBuilder<T, TBuilder>
{
    protected ReadWriteRepositoryBase(OdkContext context)
        : base(context)
    {
    }

    public override void Add(T entity)
    {
        SetId(entity);

        base.Add(entity);
    }

    public override void AddMany(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            SetId(entity);
        }

        base.AddMany(entities);
    }

    public virtual IDeferredQuerySingle<T> GetById(Guid id) 
        => Query()
            .ById(id)
            .GetSingle();

    public virtual IDeferredQuerySingleOrDefault<T> GetByIdOrDefault(Guid id)
        => Query()
            .ById(id)
            .GetSingleOrDefault();

    public virtual IDeferredQueryMultiple<T> GetByIds(IReadOnlyCollection<Guid> ids)
        => Query()
            .ByIds(ids)
            .GetAll();

    public abstract TBuilder Query();

    public void Upsert(T entity)
    {
        if (entity.Id == default)
        {
            Add(entity);
        }
        else
        {
            Update(entity);
        }
    }

    private static void SetId(T entity)
    {
        if (entity.Id == default)
        {
            entity.Id = Guid.NewGuid();
        }
    }
}