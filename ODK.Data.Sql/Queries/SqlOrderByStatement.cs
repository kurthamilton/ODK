using System;
using System.Linq.Expressions;

namespace ODK.Data.Sql.Queries
{
    public class SqlOrderByStatement<T, TValue> : ISqlComponent
    {
        public SqlOrderByStatement(Expression<Func<T, TValue>> expression, SqlSortDirection direction = SqlSortDirection.Ascending)
        {
            Direction = direction;
            Expression = expression;
        }

        private SqlSortDirection Direction { get; }

        private Expression<Func<T, TValue>> Expression { get; }

        public string ToSql(SqlContext context)
        {
            SqlColumn column = context.GetColumn(Expression);
            return $"{column.ToSql(context)} {(Direction == SqlSortDirection.Descending ? "DESC" : "ASC")}";
        }
    }
}
