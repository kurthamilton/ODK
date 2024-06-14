using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ODK.Data.Sql.Queries
{
    public class SqlQueryCondition<T, TEntity, TValue, TQuery> : ISqlQueryCondition 
        where TQuery : SqlQuery<T>
    {
        public SqlQueryCondition(TQuery query, Expression<Func<TEntity, TValue>> expression)
        {
            Expression = expression;
            Query = query;
        }
        
        private Expression<Func<TEntity, TValue>> Expression { get; }

        private string? Operator { get; set; }

        private string? Parameter { get; set; }

        private IDictionary<string, TValue> Values { get; } = new Dictionary<string, TValue>();

        // Only used for IN operator
        private List<string> Parameters { get; } = new List<string>();

        private TQuery Query { get; }

        public object? GetValue(string parameter)
        {
            return Values.TryGetValue(parameter, out TValue value)
                ? value 
                : default;
        }

        public TQuery In(IEnumerable<TValue> values)
        {
            Operator = "IN";

            foreach (TValue value in values)
            {
                string parameter = Query.AddParameterValue(Expression, value);
                Parameters.Add(parameter);
                Values[parameter] = value;
            }

            return Query;
        }

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

            string columnSql = column.ToSql(context);

            if (Operator != "IN")
            {
                return $"{columnSql} {Operator} {Parameter}".Trim();
            }

            return $"{columnSql} {Operator} ({string.Join(",", Parameters)})".Trim();
        }

        private TQuery SetCondition(string @operator, TValue value)
        {
            SetCondition(@operator);
            Parameter = Query.AddParameterValue(Expression, value);
            Values[Parameter] = value;
            return Query;
        }

        private TQuery SetCondition(string @operator)
        {
            Operator = @operator;
            return Query;
        }
    }
}
