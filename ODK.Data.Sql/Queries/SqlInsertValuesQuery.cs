using System.Linq;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Queries
{
    public class SqlInsertValuesQuery<T> : SqlQuery<T>
    {
        public SqlInsertValuesQuery(SqlContext context)
            : base(context)
        {
        }

        public SqlInsertValuesQuery<T> Value(T entity)
        {
            BuildSql();
            AddParameterValues(entity);
            return this;
        }

        private void AddParameterValues(T instance)
        {
            SqlMap<T> map = Context.GetMap<T>();

            foreach (SqlColumn column in map.InsertColumns)
            {
                string entityFieldName = map.GetEntityFieldName(column);
                object value = instance.GetType().GetProperty(entityFieldName)?.GetValue(instance);

                AddParameterValue(column, value);
            }
        }

        private void BuildSql()
        {
            SqlMap<T> map = Context.GetMap<T>();

            AppendSql($"INSERT INTO {map.TableName}");
            AppendSql(" (");
            AppendSql(map.InsertColumnSql);
            AppendSql(") ");
            AppendSql(" VALUES (");
            AppendSql(string.Join(",", map.InsertColumns.Select(x => x.ParameterName)));
            AppendSql(")");
        }
    }
}
