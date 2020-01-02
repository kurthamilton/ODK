using System;
using System.Linq.Expressions;

namespace ODK.Data.Sql.Queries
{
    public class SqlUpdateQuery<T> : SqlConditionalQuery<T, SqlUpdateQuery<T>>
    {
        public SqlUpdateQuery(SqlContext context)
            : base(context)
        {
        }

        protected override SqlUpdateQuery<T> Query => this;

        public SqlUpdateQuery<T> Set<TValue>(Expression<Func<T, TValue>> expression, TValue value)
        {
            AddUpdateColumn(expression, value);
            return this;
        }
    }
}
