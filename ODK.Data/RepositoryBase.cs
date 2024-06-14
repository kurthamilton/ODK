using ODK.Data.Sql;

namespace ODK.Data;

public abstract class RepositoryBase
{
    protected RepositoryBase(SqlContext context)
    {
        Context = context;
    }

    protected SqlContext Context { get; }
}
