using System.Linq;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Queries
{
    public class SqlInsertValuesQuery<T> : SqlQuery<T>
    {
        public SqlInsertValuesQuery(SqlContext context)
            : base(context)
        {
        }

        public SqlInsertValuesQuery<T> OutputIdentity()
        {
            SqlMap<T> map = Context.GetMap<T>();
            SqlColumn identity = map.SelectColumns.FirstOrDefault(x => x.Identity);
            if (identity != null)
            {
                AddInsertOutput(identity);
            }

            return this;
        }

        public SqlInsertValuesQuery<T> Value(T entity)
        {
            AddInsertEntity(entity);
            return this;
        }
    }
}
