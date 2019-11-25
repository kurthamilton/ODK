using System;
using System.Linq.Expressions;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql
{
    public class SqlJoin<TFrom, TTo, TValue>
    {
        public SqlJoin(SqlContext context, Expression<Func<TFrom, TValue>> fromExpression, Expression<Func<TTo, TValue>> toExpression)
        {
            SqlMap<TFrom> fromMap = context.GetMap<TFrom>();
            SqlMap<TTo> toMap = context.GetMap<TTo>();

            SqlColumn fromColumn = fromMap.GetColumn(fromExpression);
            SqlColumn toColumn = toMap.GetColumn(toExpression);

            Sql = $" JOIN {toMap.TableName} ON {fromColumn.ToSql()} = {toColumn.ToSql()}";
        }

        private string Sql { get; }

        public string ToSql()
        {
            return Sql;
        }
    }
}
