using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.QueryBuilders;

public interface IQueryBuilder<T>
{
    IDeferredQuery<int> Count();

    IDeferredQueryMultiple<T> GetAll();

    IDeferredQuerySingle<T> GetSingle();

    IDeferredQuerySingleOrDefault<T> GetSingleOrDefault();
}