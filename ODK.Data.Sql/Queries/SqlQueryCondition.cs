using System;
using System.Linq.Expressions;

namespace ODK.Data.Sql.Queries
{
    public class SqlQueryCondition<T, TEntity, TValue> : ISqlQueryCondition
    {
        public SqlQueryCondition(SqlConditionalQuery<T> query, Expression<Func<TEntity, TValue>> expression)
        {
            Expression = expression;
            Query = query;
        }

        public object Value { get; private set; }

        private Expression<Func<TEntity, TValue>> Expression { get; }

        private string Operator { get; set; }

        private SqlConditionalQuery<T> Query { get; }

        public SqlConditionalQuery<T> EqualTo(TValue value)
        {
            return SetCondition("=", value);
        }

        public SqlColumn GetColumn(SqlContext context)
        {
            return context.GetColumn(Expression);
        }

        public SqlConditionalQuery<T> GreaterThan(TValue value)
        {
            return SetCondition(">", value);
        }

        public SqlConditionalQuery<T> GreaterThanOrEqualTo(TValue value)
        {
            return SetCondition(">=", value);
        }

        public string ToSql(SqlContext context)
        {
            SqlColumn column = context.GetColumn(Expression);

            return $"{column.ToSql()} {Operator} {column.ParameterName}";
        }

        private SqlConditionalQuery<T> SetCondition(string @operator, TValue value)
        {
            Operator = @operator;
            Value = value;
            return Query;
        }
    }
}
