using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class QueryBuilder<T> : IQueryBuilder<T>
    where T : class
{
    private readonly OdkContext _context;

    protected IQueryable<T> _query;

    internal QueryBuilder(OdkContext context)
    {
        _context = context;
        _query = Set();
    }

    public IDeferredQueryMultiple<T> GetAll() => _query.DeferredMultiple();

    public IDeferredQuerySingle<T> GetSingle() => _query.DeferredSingle();

    public IDeferredQuerySingleOrDefault<T> GetSingleOrDefault() => _query.DeferredSingleOrDefault();

    protected virtual IQueryable<T> Set() => Set<T>();

    protected IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
        => _context.Set<TEntity>();
}