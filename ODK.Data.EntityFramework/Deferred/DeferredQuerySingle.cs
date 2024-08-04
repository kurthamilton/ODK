using ODK.Core;
using ODK.Core.Exceptions;
using ODK.Data.Core.Deferred;
using Z.EntityFramework.Plus;

namespace ODK.Data.EntityFramework.Deferred;
public class DeferredQuerySingle<T> : IDeferredQuerySingle<T> where T : class
{
    private readonly T? _cached = null;
    private readonly QueryFutureValue<T>? _query = null;
    private Action<T>? _updateCache = null;

    internal DeferredQuerySingle(IQueryable<T> query)
    {
        _query = query
            .DeferredFirstOrDefault()
            .FutureValue();
    }

    internal DeferredQuerySingle(
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

    public async Task<T> RunAsync()
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
        OdkAssertions.Exists(value);

        _updateCache?.Invoke(value);

        return value;
    }
}
