using ODK.Core.Exceptions;
using ODK.Data.Core.Deferred;
using Z.EntityFramework.Plus;

namespace ODK.Data.EntityFramework.Deferred;
public class DeferredQuerySingle<T> : IDeferredQuerySingle<T> where T : class
{
    private readonly QueryFutureValue<T> _query;

    internal DeferredQuerySingle(IQueryable<T> query)
    {
        _query = query
            .DeferredFirstOrDefault()
            .FutureValue();
    }

    public async Task<T> RunAsync()
    {
        var value = await _query.ValueAsync();
        if (value == null)
        {
            throw new OdkNotFoundException();
        }
        return value;
    }

}
