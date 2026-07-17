using ODK.Core;
using ODK.Data.Core.Deferred;
using Z.EntityFramework.Plus;

namespace ODK.Data.EntityFramework.Deferred;

public class DeferredQuerySingle<T> : IDeferredQuerySingle<T> where T : class
{
    private readonly Guid? _id;
    private readonly QueryFutureValue<T> _query;

    internal DeferredQuerySingle(IQueryable<T> query, Guid? id = null)
    {
        _id = id;

        _query = query
            .DeferredFirstOrDefault()
            .FutureValue();
    }

    public async Task<T> Run()
    {
        var value = await _query.ValueAsync();

        var errorMessage = $"Database entity {typeof(T).Name} not found";
        if (_id != null)
        {
            errorMessage += $" for id '{_id}'";
        }

        OdkAssertions.Exists(value, errorMessage);

        return value;
    }
}
