using ODK.Core;

namespace ODK.Data.Core.QueryBuilders;

public interface IDatabaseEntityQueryBuilder<T> : IDatabaseEntityQueryBuilder<T, IDatabaseEntityQueryBuilder<T>>
    where T : IDatabaseEntity
{
}

public interface IDatabaseEntityQueryBuilder<T, TBuilder> : IQueryBuilder<T>
    where T : IDatabaseEntity
    where TBuilder : IQueryBuilder<T>
{
    TBuilder ById(Guid id);

    TBuilder ByIds(IEnumerable<Guid> ids);
}