using System;
using System.Linq.Expressions;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql
{
    public class SqlJoin<TFrom, TTo, TValue> : ISqlComponent
    {
        private readonly Expression<Func<TFrom, TValue>> _fromExpression;
        private readonly Expression<Func<TTo, TValue>> _toExpression;

        public SqlJoin(Expression<Func<TFrom, TValue>> fromExpression, Expression<Func<TTo, TValue>> toExpression)
        {
            _fromExpression = fromExpression;
            _toExpression = toExpression;
        }

        public string ToSql(SqlContext context)
        {
            SqlMap<TFrom> fromMap = context.GetMap<TFrom>();
            SqlMap<TTo> toMap = context.GetMap<TTo>();

            SqlColumn fromColumn = fromMap.GetColumn(_fromExpression);
            SqlColumn toColumn = toMap.GetColumn(_toExpression);

            return $" JOIN {toMap.TableName} ON {fromColumn.ToSql()} = {toColumn.ToSql()}";
        }
    }
}
