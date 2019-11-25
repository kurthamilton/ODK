using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Queries
{
    public abstract class SqlQuery<T>
    {
        private readonly IList<string> _conditions = new List<string>();
        private bool _delete;
        private string _from = "";
        private readonly IList<ISqlComponent> _joins = new List<ISqlComponent>();
        private readonly IList<string> _insertColumns = new List<string>();
        private readonly IList<string> _joins = new List<string>();
        private readonly IList<string> _orderByFields = new List<string>();
        private readonly IList<string> _selectColumns = new List<string>();
        private readonly IList<SqlColumn> _updateColumns = new List<SqlColumn>();

        protected SqlQuery(SqlContext context)
        {
            Context = context;
        }

        protected SqlContext Context { get; }

        private IList<(SqlColumn column, object value)> ParameterValues { get; } = new List<(SqlColumn, object)>();

        public async Task ExecuteAsync()
        {
            await Context.ExecuteNonQueryAsync(this);
        }

        public async Task<int> CountAsync()
        {
            _selectColumns.Clear();
            AddSelectColumn("COUNT(*)");
            return await Context.ReadRecordAsync(this, reader => reader.GetInt32(0));
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            return await Context.ReadRecordAsync(this);
        }

        public IEnumerable<(SqlColumn, object)> GetParameterValues()
        {
            return ParameterValues.ToArray();
        }

        public async Task<IReadOnlyCollection<T>> ToArrayAsync()
        {
            return await Context.ReadRecordsAsync(this);
        }

        public string ToSql()
        {
            return InitialClauseSql() +
               FromSql() +
               JoinSql() +
               WhereSql() +
               OrderBySql();
        }

        protected void AddCondition<TEntity, TValue>(Expression<Func<TEntity, TValue>> expression, string @operator, TValue value)
        {
            SqlMap<TEntity> map = Context.GetMap<TEntity>();
            SqlColumn column = map.GetColumn(expression);

            _conditions.Add($"{column.ToSql()} {@operator} {column.ParameterName}");

            AddParameterValue(column, value);
        }

        protected void AddDelete()
        {
            _delete = true;
        }

        protected void AddFrom()
        {
            SqlMap<T> map = Context.GetMap<T>();
            _from = map.TableName;
        }

        protected void AddInsertColumns(T entity)
        {
            SqlMap<T> map = Context.GetMap<T>();

            foreach (SqlColumn column in map.InsertColumns)
            {
                string entityFieldName = map.GetEntityFieldName(column);
                object value = entity.GetType().GetProperty(entityFieldName)?.GetValue(entity);

                _insertColumns.Add(column.ColumnName);
                AddParameterValue(column, value);
            }
        }

        protected void AddJoin<TFrom, TTo, TValue>(Expression<Func<TFrom, TValue>> fromField, Expression<Func<TTo, TValue>> toField)
        {
            SqlJoin<TFrom, TTo, TValue> join = new SqlJoin<TFrom, TTo, TValue>(fromField, toField);
            _joins.Add(join);
        }

        protected void AddOrderBy<TValue>(Expression<Func<T, TValue>> expression, string direction = null)
        {
            SqlMap<T> map = Context.GetMap<T>();

            SqlColumn column = map.GetColumn(expression);

            string orderBy = column.ToSql();
            if (string.Equals(direction, "DESC", StringComparison.OrdinalIgnoreCase))
            {
                orderBy += " DESC";
            }

            _orderByFields.Add(orderBy);
        }

        protected void AddSelectColumn(string column)
        {
            _selectColumns.Add(column);
        }

        protected void AddSelectColumns(IEnumerable<string> columns)
        {
            foreach (string column in columns)
            {
                AddSelectColumn(column);
            }
        }

        protected void AddUpdateColumn<TValue>(Expression<Func<T, TValue>> expression, TValue value)
        {
            SqlMap<T> map = Context.GetMap<T>();

            // TODO: resolve conflict between set parameter and where parameter when both operate on same column
            SqlColumn column = map.GetColumn(expression);
            AddParameterValue(column, value);
            _updateColumns.Add(column);
        }

        private static string UpdateColumnSql(SqlColumn column)
        {
            return $"{column.ToSql()} = {column.ParameterName}";
        }

        private void AddParameterValue(SqlColumn column, object value)
        {
            ParameterValues.Add((column, value));
        }

        private string FromSql()
        {
            return _from.Length > 0 ? $" FROM {_from}" : "";
        }

        private string InitialClauseSql()
        {
            if (_selectColumns.Count > 0)
            {
                return $"SELECT {string.Join(",", _selectColumns)}";
            }

            SqlMap<T> map = Context.GetMap<T>();

            if (_updateColumns.Count > 0)
            {

                return $"UPDATE {map.TableName}" +
                       $"SET {string.Join(",", _updateColumns.Select(UpdateColumnSql))}";
            }

        private string JoinSql()
        {
            SqlMap<T> map = Context.GetMap<T>();
            return string.Join(" ", map.Joins.Union(_joins).Select(x => x.ToSql(Context)));
        }
            if (_insertColumns.Count > 0)
            {
                return $"INSERT INTO {map.TableName} ({string.Join(",", _insertColumns)}) " +
                       $"VALUES({ string.Join(",", map.InsertColumns.Select(x => x.ParameterName))})";
            }

            if (_delete)
            {
                return $"DELETE {map.TableName}";
            }

            throw new InvalidOperationException("Invalid SQL Query");
        }

        private string JoinSql()
        {
            return string.Join(" ", _joins);
        }

        private string OrderBySql()
        {
            return _orderByFields.Count > 0 ? $" ORDER BY {string.Join(",", _orderByFields)}" : "";
        }

        private string WhereSql()
        {
            return _conditions.Count > 0 ? $" WHERE {string.Join(" AND ", _conditions)}" : "";
        }
    }
}
