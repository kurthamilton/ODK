using System.Linq.Expressions;
using ODK.Data.Core.Deferred;
using ODK.Data.EntityFramework.Deferred;

namespace ODK.Data.EntityFramework.Extensions;

internal static class QueryableExtensions
{
    internal static IQueryable<T> ConditionalWhere<T>(this IQueryable<T> query,
        Expression<Func<T, bool>> condition,
        bool apply)
    {
        return apply
            ? query.Where(condition)
            : query;
    }

    internal static IDeferredQuery<bool> DeferredAny<T>(this IQueryable<T> query) where T : class
        => new DeferredQueryAny<T>(query);

    internal static IDeferredQuery<int> DeferredCount<T>(this IQueryable<T> query) where T : class
        => new DeferredQueryCount<T>(query);

    internal static IDeferredQueryMultiple<T> DeferredMultiple<T>(this IQueryable<T> query) where T : class
        => new DeferredQueryMultiple<T>(query);

    internal static IDeferredQueryMultiple<T> DeferredMultiple<T>(
        this IQueryable<T> query,
        Func<IReadOnlyCollection<T>?> getFromCache,
        Action<IReadOnlyCollection<T>> updateCache)
        where T : class
        => new DeferredQueryMultiple<T>(query, getFromCache, updateCache);

    internal static IDeferredQuerySingle<T> DeferredSingle<T>(this IQueryable<T> query) where T : class
        => new DeferredQuerySingle<T>(query);

    internal static IDeferredQuerySingle<T> DeferredSingle<T>(
        this IQueryable<T> query,
        Func<T?> getFromCache,
        Action<T> updateCache,
        Action<IEnumerable<T>>? prefillCache = null)
        where T : class
        => new DeferredQuerySingle<T>(query, getFromCache, updateCache, prefillCache);

    internal static IDeferredQuerySingleOrDefault<T> DeferredSingleOrDefault<T>(this IQueryable<T> query) where T : class
        => new DeferredQuerySingleOrDefault<T>(query);

    internal static IDeferredQuerySingleOrDefault<T> DeferredSingleOrDefault<T>(
        this IQueryable<T> query,
        Func<T?> getFromCache,
        Action<T> updateCache,
        Action<IEnumerable<T>>? prefillCache = null)
        where T : class
        => new DeferredQuerySingleOrDefault<T>(query, getFromCache, updateCache);

    internal static IQueryable<T> Page<T>(this IQueryable<T> query, int page, int pageSize)
    {
        var pageIndex = page - 1;
        return query
            .Skip(pageIndex * pageSize)
            .Take(pageSize);
    }
}
