using System;
using System.Linq.Expressions;

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
            SqlColumn fromColumn = context.GetColumn(_fromExpression);
            SqlColumn toColumn = context.GetColumn(_toExpression);

            return $" JOIN {context.GetTableName<TTo>()} ON {fromColumn.ToSql()} = {toColumn.ToSql()}";
        }
    }
}
