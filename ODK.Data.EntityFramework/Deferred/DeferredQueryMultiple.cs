using ODK.Data.Core.Deferred;
using Z.EntityFramework.Plus;

namespace ODK.Data.EntityFramework.Deferred;
public class DeferredQueryMultiple<T> : IDeferredQueryMultiple<T> where T : class
{
    private readonly QueryFutureEnumerable<T> _query;

    internal DeferredQueryMultiple(IQueryable<T> query)
    {
        _query = query.Future();
    }

    public async Task<IReadOnlyCollection<T>> RunAsync() => await _query.ToArrayAsync();
}
