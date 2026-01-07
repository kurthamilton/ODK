using ODK.Data.Core.Deferred;
using Z.EntityFramework.Plus;

namespace ODK.Data.EntityFramework.Deferred;

public class DeferredQuerySingleOrDefault<T> : IDeferredQuerySingleOrDefault<T> where T : class
{
    private readonly T? _cached = null;
    private readonly Func<T?>? _getFromCache = null;
    private readonly Action<IEnumerable<T>>? _prefillCache = null;
    private readonly QueryFutureValue<T>? _query;
    private readonly QueryFutureEnumerable<T>? _queryAll = null;
    private Action<T>? _updateCache = null;

    internal DeferredQuerySingleOrDefault(IQueryable<T> query)
    {
        _query = query
            .DeferredFirstOrDefault()
            .FutureValue();
    }

    internal DeferredQuerySingleOrDefault(
        IQueryable<T> query,
        Func<T?> getFromCache,
        Action<T> updateCache,
        Action<IEnumerable<T>>? prefillCache = null)
    {
        _cached = getFromCache();
        if (_cached != null)
        {
            return;
        }

        if (prefillCache != null)
        {
            _queryAll = query
                .Future();
        }
        else
        {
            _query = query
                .DeferredFirstOrDefault()
                .FutureValue();
        }

        _getFromCache = getFromCache;
        _prefillCache = prefillCache;
        _updateCache = updateCache;
    }

    public async Task<T?> Run()
    {
        if (_cached != null)
        {
            return _cached;
        }

        if (_query == null)
        {
            throw new Exception("Query not set");
        }

        T? value = null;
        if (_query != null)
        {
            value = await _query.ValueAsync();
        }
        else if (_queryAll != null && _prefillCache != null && _getFromCache != null)
        {
            var values = await _queryAll.ToArrayAsync();
            _prefillCache.Invoke(values);
            value = _getFromCache();
        }
        else
        {
            throw new Exception("Query not set");
        }

        return value;
    }
}
