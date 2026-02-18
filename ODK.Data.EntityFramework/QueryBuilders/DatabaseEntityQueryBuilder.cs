using ODK.Core;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class DatabaseEntityQueryBuilder<T> : 
    DatabaseEntityQueryBuilder<T, IDatabaseEntityQueryBuilder<T>>, 
    IDatabaseEntityQueryBuilder<T, IDatabaseEntityQueryBuilder<T>>,
    IDatabaseEntityQueryBuilder<T>
    where T : class, IDatabaseEntity
{
    public DatabaseEntityQueryBuilder(OdkContext context) 
        : base(context)
    {
    }

    protected override IDatabaseEntityQueryBuilder<T> Builder => this;
}

public abstract class DatabaseEntityQueryBuilder<T, TBuilder> : QueryBuilder<T>, IDatabaseEntityQueryBuilder<T, TBuilder>
    where T : class, IDatabaseEntity
    where TBuilder : IQueryBuilder<T>
{
    internal DatabaseEntityQueryBuilder(OdkContext context)
        : base(context)
    {
    }

    protected abstract TBuilder Builder { get; }

    public TBuilder ById(Guid id)
    {
        Query = Query.Where(x => x.Id == id);
        return Builder;
    }

    public TBuilder ByIds(IEnumerable<Guid> ids)
    {
        Query = Query.Where(x => ids.Contains(x.Id));
        return Builder;
    }
}
