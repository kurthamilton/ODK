using ODK.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework;

public abstract class CachingReadWriteRepositoryBase<T> : CachingWriteRepositoryBase<T, Guid>, IReadWriteRepository<T>
    where T : class, IDatabaseEntity
{
    private readonly EntityCache<Guid, T> _cache;

    protected CachingReadWriteRepositoryBase(OdkContext context, EntityCache<Guid, T> cache)
        : base(context, cache)
    {
        _cache = cache;
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

    public virtual IDeferredQuerySingle<T> GetById(Guid id) => Set()
        .Where(x => x.Id == id)
        .DeferredSingle(
            () => _cache.Get(id),
            _cache.Set);

    public virtual IDeferredQuerySingleOrDefault<T> GetByIdOrDefault(Guid id) => Set()
        .Where(x => x.Id == id)
        .DeferredSingleOrDefault(
            () => _cache.Get(id),
            _cache.Set);

    public IDeferredQueryMultiple<T> GetByIds(IReadOnlyCollection<Guid> ids) =>
        ids.Count > 0
            ? Set()
                .Where(x => ids.Contains(x.Id))
                .DeferredMultiple()
            : new DefaultDeferredQueryMultiple<T>();

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
