using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Queries
{
    public class SqlDeleteQuery<T> : SqlConditionalQuery<T>
    {
        public SqlDeleteQuery(SqlContext context)
            : base(context)
        {
            SqlMap<T> map = context.GetMap<T>();

            AppendSql($"DELETE {map.TableName}");
            AddFrom();
        }
    }
}
