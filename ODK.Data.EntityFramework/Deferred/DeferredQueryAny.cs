using ODK.Data.Core.Deferred;
using Z.EntityFramework.Plus;

namespace ODK.Data.EntityFramework.Deferred;
public class DeferredQueryAny<T> : IDeferredQuerySingle<bool> where T : class
{
    private readonly QueryFutureValue<bool> _query;

    internal DeferredQueryAny(IQueryable<T> query)
    {
        _query = query
            .DeferredAny()
            .FutureValue();
    }

    public async Task<bool> RunAsync() => await _query.ValueAsync();
}
