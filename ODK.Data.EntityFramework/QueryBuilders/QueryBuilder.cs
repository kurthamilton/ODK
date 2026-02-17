using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class QueryBuilder<T> : IQueryBuilder<T>
    where T : class
{
    private readonly OdkContext _context;

    internal QueryBuilder(OdkContext context)
    {
        _context = context;
        Query = Set();
    }

    private QueryBuilder(OdkContext context, IQueryable<T> query)
    {
        _context = context;
        Query = query;
    }

    protected IQueryable<T> Query { get; set; }

    public IDeferredQuery<int> Count() => Query.DeferredCount();

    public IDeferredQueryMultiple<T> GetAll() => Query.DeferredMultiple();

    public IDeferredQuerySingle<T> GetSingle() => Query.DeferredSingle();

    public IDeferredQuerySingleOrDefault<T> GetSingleOrDefault() => Query.DeferredSingleOrDefault();

    protected IQueryBuilder<TDto> ProjectTo<TDto>(IQueryable<TDto> query)
        where TDto : class
        => new QueryBuilder<TDto>(_context, query);

    protected virtual IQueryable<T> Set() => Query;

    protected IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
        => _context.Set<TEntity>();
}