using System;
using System.Linq;
using System.Linq.Expressions;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Queries
{
    public class SqlSelectQuery<T> : SqlConditionalQuery<T>
    {
        public SqlSelectQuery(SqlContext context)
            : base(context)
        {
            SqlMap<T> map = Context.GetMap<T>();
            AddSelectColumns(map.SelectColumns.Select(x => x.ToSql()));
            AddFrom();
        }

        public SqlSelectQuery<T> Join<TTo, TValue>(Expression<Func<T, TValue>> thisField, Expression<Func<TTo, TValue>> toField)
        {
            return Join<T, TTo, TValue>(thisField, toField);
        }

        public SqlSelectQuery<T> Join<TFrom, TTo, TValue>(Expression<Func<TFrom, TValue>> fromField, Expression<Func<TTo, TValue>> toField)
        {
            AddJoin(fromField, toField);
            return this;
        }

        public SqlSelectQuery<T> OrderBy<TValue>(Expression<Func<T, TValue>> expression, string direction = null)
        {
            AddOrderBy(expression, direction);
            return this;
        }
    }
}
