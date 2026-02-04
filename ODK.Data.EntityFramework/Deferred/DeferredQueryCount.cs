using ODK.Data.Core.Deferred;
using Z.EntityFramework.Plus;

namespace ODK.Data.EntityFramework.Deferred;

public class DeferredQueryCount<T> : IDeferredQuerySingle<int>
{
    private readonly QueryFutureValue<int> _query;

    internal DeferredQueryCount(IQueryable<T> query)
    {
        _query = query
            .DeferredCount()
            .FutureValue();
    }

    public async Task<int> Run() => await _query.ValueAsync();
}