using System;
using System.Linq;
using System.Linq.Expressions;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Queries
{
    public class SqlSelectQuery<T> : SqlConditionalQuery<T>
    {
        public SqlSelectQuery(SqlContext context)
            : base(context)
        {
            SqlMap<T> map = Context.GetMap<T>();
            AddSelectColumns(map.SelectColumns.Select(x => x.ToSql()));
        }

        public SqlSelectQuery<T> OrderBy<TValue>(Expression<Func<T, TValue>> expression, SqlSortDirection direction = SqlSortDirection.Ascending)
        {
            AddOrderBy(expression, direction);
            return this;
        }

        public SqlSelectQuery<T> Top(int size)
        {
            AddTop(size);
            return this;
        }
    }
}
