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

        private string Parameter { get; set; }

        private TQuery Query { get; }

        public TQuery EqualTo(TValue value)
        {
            return SetCondition("=", value);
        }

        public TQuery GreaterThan(TValue value)
        {
            return SetCondition(">", value);
        }

        public TQuery GreaterThanOrEqualTo(TValue value)
        {
            return SetCondition(">=", value);
        }

        public TQuery IsNotNull()
        {
            return SetCondition("IS NOT NULL");
        }

        public TQuery LessThan(TValue value)
        {
            return SetCondition("<", value);
        }

        public TQuery LessThanOrEqualTo(TValue value)
        {
            return SetCondition("<=", value);
        }

        public TQuery NotEqualTo(TValue value)
        {
            return SetCondition("!=", value);
        }

        public string ToSql(SqlContext context)
        {
            SqlColumn column = context.GetColumn(Expression);

            return $"{column.ToSql(context)} {Operator} {Parameter}".Trim();
        }

        private TQuery SetCondition(string @operator, TValue value)
        {
            SetCondition(@operator);
            Value = value;
            Parameter = Query.AddParameterValue(Expression, value);
            return Query;
        }

        private TQuery SetCondition(string @operator)
        {
            Operator = @operator;
            return Query;
        }
    }
}
