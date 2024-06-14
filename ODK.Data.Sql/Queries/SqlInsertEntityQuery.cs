using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Queries;

public class SqlInsertEntityQuery<T> : SqlQuery<T>
{
    public SqlInsertEntityQuery(SqlContext context, T entity)
        : base(context)
    {
        AddInsertEntity(entity);
    }

    public void OutputIdentity()
    {
        SqlMap<T> map = Context.GetMap<T>();            
        SqlColumn identity = map.SelectColumns.FirstOrDefault(x => x.Identity);
        if (identity != null)
        {
            AddInsertOutput(identity);
        }
    }

    public async Task<Guid> GetIdentityAsync()
    {
        OutputIdentity();
        return await Context.ReadRecordAsync(this, reader => reader.GetGuid(0));
    }
}
