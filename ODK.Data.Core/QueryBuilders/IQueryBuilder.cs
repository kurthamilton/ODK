using System.Linq.Expressions;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.QueryBuilders;

public interface IQueryBuilder<T>
{
    IDeferredQuery<bool> Any();

    IDeferredQuery<int> Count();

    IDeferredQueryMultiple<T> GetAll();

    IDeferredQuerySingle<T> GetSingle();

    IDeferredQuerySingleOrDefault<T> GetSingleOrDefault();

    IQueryBuilder<T> OrderBy<TOrderBy>(Expression<Func<T, TOrderBy>> orderBy);

    IQueryBuilder<T> OrderByDescending<TOrderBy>(Expression<Func<T, TOrderBy>> orderBy);

    IQueryBuilder<T> Page(int page, int pageSize);

    IQueryBuilder<T> Take(int count);
}