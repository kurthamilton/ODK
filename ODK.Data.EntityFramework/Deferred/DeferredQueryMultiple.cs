using ODK.Data.Core.Deferred;
using Z.EntityFramework.Plus;

namespace ODK.Data.EntityFramework.Deferred;
public class DeferredQueryMultiple<T> : IDeferredQueryMultiple<T> where T : class
{
    private readonly IReadOnlyCollection<T>? _cached = null;
    private readonly QueryFutureEnumerable<T>? _query;
    private readonly Action<IReadOnlyCollection<T>>? _updateCache = null;

    internal DeferredQueryMultiple(IQueryable<T> query)
    {
        _query = query.Future();
    }

    internal DeferredQueryMultiple(
        IQueryable<T> query,
        Func<IReadOnlyCollection<T>?> getFromCache,
        Action<IReadOnlyCollection<T>> updateCache)
    {
        _cached = getFromCache();
        if (_cached != null)
        {
            return;
        }

        _query = query.Future();
        _updateCache = updateCache;
    }

    public async Task<IReadOnlyCollection<T>> Run()
    {        
        if (_cached != null)
        {
            return _cached;
        }

        if (_query == null)
        {
            return [];
        }

        var result = await _query.ToArrayAsync();
        _updateCache?.Invoke(result);
        return result;
    }  
}
