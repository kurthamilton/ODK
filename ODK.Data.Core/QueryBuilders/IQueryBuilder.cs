using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.QueryBuilders;

public interface IQueryBuilder<T>
{
    IDeferredQueryMultiple<T> GetAll();

    IDeferredQuerySingle<T> GetSingle();

    IDeferredQuerySingleOrDefault<T> GetSingleOrDefault();
}