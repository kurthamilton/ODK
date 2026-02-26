using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class QueryBuilder<T> : IQueryBuilder<T>
    where T : class
{
    private readonly DbContext _context;

    protected QueryBuilder(DbContext context)
        : this(context, context.Set<T>())
    {
    }

    protected QueryBuilder(DbContext context, IQueryable<T> query)
    {
        _context = context;
        Query = query;
    }

    protected IQueryable<T> Query { get; set; }

    public IDeferredQuery<bool> Any() => Query.DeferredAny();

    public IDeferredQuery<int> Count() => Query.DeferredCount();

    public IDeferredQueryMultiple<T> GetAll() => Query.DeferredMultiple();

    public IDeferredQueryMultiple<T> GetOrdered<TOrderBy>(Expression<Func<T, TOrderBy>> orderBy, bool descending)
        => descending
            ? Query.OrderByDescending(orderBy).DeferredMultiple()
            : Query.OrderBy(orderBy).DeferredMultiple();

    public IDeferredQuerySingle<T> GetSingle() => Query.DeferredSingle();

    public IDeferredQuerySingleOrDefault<T> GetSingleOrDefault() => Query.DeferredSingleOrDefault();

    public IQueryBuilder<T> OrderBy<TOrderBy>(Expression<Func<T, TOrderBy>> orderBy)
    {
        Query = Query.OrderBy(orderBy);
        return this;
    }

    public IQueryBuilder<T> OrderByDescending<TOrderBy>(Expression<Func<T, TOrderBy>> orderBy)
    {
        Query = Query.OrderByDescending(orderBy);
        return this;
    }

    public IQueryBuilder<T> Page(int page, int pageSize)
    {
        Query = Query.Page(page, pageSize);
        return this;
    }

    public IQueryBuilder<T> Take(int count)
    {
        Query = Query.Take(count);
        return this;
    }

    protected TBuilder CreateQueryBuilder<TBuilder, TNew>(Func<DbContext, TBuilder> createBuilder)
        where TNew : class
        where TBuilder : IQueryBuilder<TNew>
        => createBuilder(_context);

    protected IQueryBuilder<TDto> ProjectTo<TDto>(IQueryable<TDto> query)
        where TDto : class
        => CreateQueryBuilder<IQueryBuilder<TDto>, TDto>(context => new QueryBuilder<TDto>(context, query));

    protected IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
        => _context.Set<TEntity>();
}