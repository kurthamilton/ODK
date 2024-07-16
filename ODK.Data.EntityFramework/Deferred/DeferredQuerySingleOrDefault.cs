using ODK.Data.Core.Deferred;
using Z.EntityFramework.Plus;

namespace ODK.Data.EntityFramework.Deferred;
public class DeferredQuerySingleOrDefault<T> : IDeferredQuerySingleOrDefault<T> where T : class
{
    private readonly QueryFutureValue<T> _query;

    internal DeferredQuerySingleOrDefault(IQueryable<T> query)
    {
        _query = query
            .DeferredFirstOrDefault()
            .FutureValue();
    }

    public async Task<T?> RunAsync() => await _query.ValueAsync();
}
