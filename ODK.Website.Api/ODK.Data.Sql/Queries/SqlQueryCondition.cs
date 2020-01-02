using System;
using System.Linq.Expressions;

namespace ODK.Data.Sql.Queries
{
    public class SqlQueryCondition<T, TEntity, TValue, TQuery> : ISqlQueryCondition where TQuery : SqlQuery<T>
    {
        public SqlQueryCondition(TQuery query, Expression<Func<TEntity, TValue>> expression)
        {
            Expression = expression;
            Query = query;
        }

        public object Value { get; private set; }

        private Expression<Func<TEntity, TValue>> Expression { get; }

        private string Operator { get; set; }

        private TQuery Query { get; }

        public TQuery EqualTo(TValue value)
        {
            return SetCondition("=", value);
        }

        public SqlColumn GetColumn(SqlContext context)
        {
            return context.GetColumn(Expression);
        }

        public TQuery GreaterThan(TValue value)
        {
            return SetCondition(">", value);
        }

        public TQuery GreaterThanOrEqualTo(TValue value)
        {
            return SetCondition(">=", value);
        }

        public TQuery NotEqualTo(TValue value)
        {
            return SetCondition("!=", value);
        }

        public string ToSql(SqlContext context)
        {
            SqlColumn column = context.GetColumn(Expression);

            return $"{column.ToSql(context)} {Operator} {column.ParameterName}";
        }

        private TQuery SetCondition(string @operator, TValue value)
        {
            Operator = @operator;
            Value = value;
            return Query;
        }
    }
}
