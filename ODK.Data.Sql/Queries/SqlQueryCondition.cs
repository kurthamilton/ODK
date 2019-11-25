using System;
using System.Linq.Expressions;
using ODK.Data.Sql.Mapping;

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
            SqlMap<TEntity> map = context.GetMap<TEntity>();
            return map.GetColumn(Expression);
        }

        public SqlConditionalQuery<T> GreaterThanOrEqualTo(TValue value)
        {
            return SetCondition(">=", value);
        }

        public string ToSql(SqlContext context)
        {
            SqlMap<TEntity> map = context.GetMap<TEntity>();
            SqlColumn column = map.GetColumn(Expression);

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
