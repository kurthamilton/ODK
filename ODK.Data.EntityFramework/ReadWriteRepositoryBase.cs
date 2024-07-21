using ODK.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework;
public abstract class ReadWriteRepositoryBase<T> : WriteRepositoryBase<T>, IReadWriteRepository<T> where T : class, IDatabaseEntity
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

    public IDeferredQuerySingle<T> GetById(Guid id) => Set()
        .Where(x => x.Id == id)
        .DeferredSingle();

    public IDeferredQuerySingleOrDefault<T> GetByIdOrDefault(Guid id) => Set()
        .Where(x => x.Id == id)
        .DeferredSingleOrDefault();

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
