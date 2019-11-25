using System;
using System.Linq.Expressions;

namespace ODK.Data.Sql.Queries
{
    public abstract class SqlConditionalQuery<T> : SqlQuery<T>
    {
        protected SqlConditionalQuery(SqlContext context)
            : base(context)
        {
        }

        public SqlConditionalQuery<T> Join<TTo, TValue>(Expression<Func<T, TValue>> thisField, Expression<Func<TTo, TValue>> toField)
        {
            return Join<T, TTo, TValue>(thisField, toField);
        }

        public SqlConditionalQuery<T> Join<TFrom, TTo, TValue>(Expression<Func<TFrom, TValue>> fromField, Expression<Func<TTo, TValue>> toField)
        {
            AddJoin(fromField, toField);
            return this;
        }

        public SqlQueryCondition<T, T, TValue> Where<TValue>(Expression<Func<T, TValue>> expression)
        {
            return Where<T, TValue>(expression);
        }

        public SqlQueryCondition<T, TEntity, TValue> Where<TEntity, TValue>(Expression<Func<TEntity, TValue>> expression)
        {
            SqlQueryCondition<T, TEntity, TValue> condition = new SqlQueryCondition<T, TEntity, TValue>(this, expression);
            AddCondition(condition);
            return condition;
        }
    }
}
