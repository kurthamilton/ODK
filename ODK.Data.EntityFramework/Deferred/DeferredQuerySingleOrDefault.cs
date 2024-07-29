using ODK.Data.Core.Deferred;
using Z.EntityFramework.Plus;

namespace ODK.Data.EntityFramework.Deferred;
public class DeferredQuerySingleOrDefault<T> : IDeferredQuerySingleOrDefault<T> where T : class
{
    private readonly T? _cached = null;
    private readonly QueryFutureValue<T>? _query;
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
        Action<T> updateCache)
    {
        _cached = getFromCache();
        if (_cached != null)
        {
            return;
        }

        _query = query
            .DeferredFirstOrDefault()
            .FutureValue();
        _updateCache = updateCache;
    }

    public async Task<T?> RunAsync()
    {
        if (_cached != null)
        {
            return _cached;
        }

        if (_query == null)
        {
            throw new Exception("Query not set");
        }

        var value = await _query.ValueAsync();
        if (value != null)
        {
            _updateCache?.Invoke(value);
        }

        return value;
    }
}
