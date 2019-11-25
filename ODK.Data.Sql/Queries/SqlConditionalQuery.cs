using System;
using System.Linq.Expressions;
using System.Text;

namespace ODK.Data.Sql.Queries
{
    public abstract class SqlConditionalQuery<T> : SqlQuery<T>
    {
        private readonly StringBuilder _sql = new StringBuilder();

        protected SqlConditionalQuery(SqlContext context)
            : base(context)
        {
        }

        public SqlConditionalQuery<T> Where<TEntity, TValue>(Expression<Func<TEntity, TValue>> expression, TValue value)
        {
            return Where(expression, "=", value);
        }

        public SqlConditionalQuery<T> Where<TValue>(Expression<Func<T, TValue>> expression, TValue value)
        {
            return Where<T, TValue>(expression, value);
        }

        public SqlConditionalQuery<T> Where<TValue>(Expression<Func<T, TValue>> expression, string @operator, TValue value)
        {
            return Where<T, TValue>(expression, @operator, value);
        }

        public SqlConditionalQuery<T> Where<TEntity, TValue>(Expression<Func<TEntity, TValue>> expression, string @operator, TValue value)
        {
            AddCondition(expression, @operator, value);
            return this;
        }        
    }
}
