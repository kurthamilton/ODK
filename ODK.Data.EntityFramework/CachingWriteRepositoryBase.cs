using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;

namespace ODK.Data.EntityFramework;
public abstract class CachingWriteRepositoryBase<T, TKey> : WriteRepositoryBase<T>, IWriteRepository<T>, IDisposable
    where T : class
    where TKey : notnull
{
    private readonly EntityCache<TKey, T> _cache;
    private readonly Guid _cacheSessionKey;

    protected CachingWriteRepositoryBase(OdkContext context, EntityCache<TKey, T> cache)
        : base(context)
    {
        _cache = cache;
        _cacheSessionKey = cache.GenerateSessionKey();
    }

    public override void Add(T entity)
    {
        _cache.SetPending(_cacheSessionKey, entity);
        base.Add(entity);
    }

    public override void AddMany(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            _cache.SetPending(_cacheSessionKey, entity);
        }

        base.AddMany(entities);
    }

    public override void Delete(T entity)
    {
        _cache.RemovePending(_cacheSessionKey, entity);
        base.Delete(entity);
    }

    public void Dispose() => _cache.EndSession(_cacheSessionKey);

    public override void Update(T entity)
    {
        _cache.SetPending(_cacheSessionKey, entity);
        base.Update(entity);
    }

    protected override void OnCommit() => _cache.CommitPending(_cacheSessionKey);
}
