using System;
using System.Linq.Expressions;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Queries
{
    public class SqlUpdateQuery<T> : SqlConditionalQuery<T>
    {
        public SqlUpdateQuery(SqlContext context)
            : base(context)
        {
            SqlMap<T> map = context.GetMap<T>();

            AppendSql($"UPDATE {map.TableName}");

            AddFrom();
        }

        public SqlUpdateQuery<T> Set<TValue>(Expression<Func<T, TValue>> expression, TValue value)
        {
            AddUpdateColumn(expression, value);
            return this;
        }        
    }
}
