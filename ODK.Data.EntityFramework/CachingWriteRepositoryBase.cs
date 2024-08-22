using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;

namespace ODK.Data.EntityFramework;
public abstract class CachingWriteRepositoryBase<T, TKey> : WriteRepositoryBase<T>, IWriteRepository<T>, IDisposable
    where T : class
    where TKey : notnull
{
    private readonly EntityCache<TKey, T> _cache;
    
    protected CachingWriteRepositoryBase(OdkContext context, EntityCache<TKey, T> cache)
        : base(context)
    {
        _cache = cache;
        CacheSessionKey = cache.GenerateSessionKey();
    }

    protected Guid CacheSessionKey { get; }

    public override void Add(T entity)
    {
        _cache.SetPending(CacheSessionKey, entity);
        base.Add(entity);
    }

    public override void AddMany(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            _cache.SetPending(CacheSessionKey, entity);
        }

        base.AddMany(entities);
    }

    public override void Delete(T entity)
    {
        _cache.RemovePending(CacheSessionKey, entity);
        base.Delete(entity);
    }

    public override void DeleteMany(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            Delete(entity);
        }
    }

    public void Dispose() => _cache.EndSession(CacheSessionKey);

    public override void Update(T entity)
    {
        _cache.SetPending(CacheSessionKey, entity);
        base.Update(entity);
    }

    protected override void OnCommit()
    {
        base.OnCommit();
        var pending = _cache.GetPending(CacheSessionKey);
        PreCommit(pending);
        _cache.CommitPending(CacheSessionKey);
    }

    protected virtual void PreCommit(IEnumerable<T> pending)
    {
    }
}
