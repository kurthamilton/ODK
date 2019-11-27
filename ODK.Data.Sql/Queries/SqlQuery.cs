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
        private readonly IList<ISqlQueryCondition> _conditions = new List<ISqlQueryCondition>();
        private bool _delete;
        private T _insertEntity;
        private SqlColumn _insertOutputColumn;
        private readonly IList<ISqlComponent> _joins = new List<ISqlComponent>();
        private readonly IList<ISqlComponent> _orderByFields = new List<ISqlComponent>();
        private readonly IList<string> _selectColumns = new List<string>();
        private readonly IList<(SqlColumn Column, object Value)> _updateColumns = new List<(SqlColumn, object)>();

        protected SqlQuery(SqlContext context)
        {
            Context = context;
        }

        protected SqlContext Context { get; }

        public async Task<int> CountAsync()
        {
            _selectColumns.Clear();
            AddSelectColumn("COUNT(*)");
            return await Context.ReadRecordAsync(this, reader => reader.GetInt32(0));
        }

        public async Task ExecuteAsync()
        {
            await Context.ExecuteNonQueryAsync(this);
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            return await Context.ReadRecordAsync(this);
        }

        public IEnumerable<(SqlColumn, object)> GetParameterValues(SqlContext context)
        {
            if (_conditions.Count > 0)
            {
                foreach (ISqlQueryCondition condition in _conditions)
                {
                    yield return (condition.GetColumn(context), condition.Value);
                }
            }

            if (_insertEntity != null)
            {
                SqlMap<T> map = context.GetMap<T>();

                foreach (SqlColumn column in map.InsertColumns)
                {
                    object value = map.GetEntityValue(_insertEntity, column);

                    yield return (column, value);
                }
            }

            if (_updateColumns.Count > 0)
            {
                foreach ((SqlColumn column, object value) updateColumn in _updateColumns)
                {
                    yield return updateColumn;
                }
            }
        }

        public async Task<IReadOnlyCollection<T>> ToArrayAsync()
        {
            return await Context.ReadRecordsAsync(this);
        }

        public string ToSql(SqlContext context)
        {
            return InitialClauseSql(context) +
               FromSql(context) +
               JoinSql(context) +
               WhereSql(context) +
               OrderBySql(context);
        }

        public async Task<int> VersionAsync()
        {
            _selectColumns.Clear();
            AddSelectColumn("CHECKSUM_AGG(CHECKSUM(*))");
            return await Context.ReadRecordAsync(this, reader => reader.GetInt32(0));
        }

        protected void AddCondition<TEntity, TValue>(SqlQueryCondition<T, TEntity, TValue> condition)
        {
            _conditions.Add(condition);
        }

        protected void AddDelete()
        {
            _delete = true;
        }

        protected void AddInsertEntity(T entity)
        {
            _insertEntity = entity;
        }

        protected void AddInsertOutput(SqlColumn column)
        {
            _insertOutputColumn = column;
        }

        protected void AddJoin<TFrom, TTo, TValue>(Expression<Func<TFrom, TValue>> fromField, Expression<Func<TTo, TValue>> toField)
        {
            SqlJoin<TFrom, TTo, TValue> join = new SqlJoin<TFrom, TTo, TValue>(fromField, toField);
            _joins.Add(join);
        }

        protected void AddOrderBy<TValue>(Expression<Func<T, TValue>> expression, SqlSortDirection direction = SqlSortDirection.Ascending)
        {
            SqlOrderByStatement<T, TValue> orderBy = new SqlOrderByStatement<T, TValue>(expression, direction);
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
            // TODO: resolve conflict between set parameter and where parameter when both operate on same column
            SqlColumn column = Context.GetColumn(expression);
            _updateColumns.Add((column, value));
        }

        private string FromSql(SqlContext context)
        {
            return _insertEntity == null ? $" FROM {context.GetTableName<T>()}" : "";
        }

        private string InitialClauseSql(SqlContext context)
        {
            if (_selectColumns.Count > 0)
            {
                return $"SELECT {string.Join(",", _selectColumns)}";
            }

            if (_updateColumns.Count > 0)
            {
                return $"UPDATE {context.GetTableName<T>()} " +
                       $"SET {string.Join(",", _updateColumns.Select(x => $"{x.Column.ToSql()} = {x.Column.ParameterName}"))}";
            }

            if (_insertEntity != null)
            {
                SqlMap<T> map = context.GetMap<T>();
                IReadOnlyCollection<SqlColumn> columns = map.InsertColumns;
                return $"INSERT INTO {context.GetTableName<T>()} ({string.Join(",", columns.Select(x => x.ColumnName))}) " +
                       (_insertOutputColumn != null ? $"OUTPUT inserted.{_insertOutputColumn.ColumnName} " : "") +
                       $"VALUES ({ string.Join(",", columns.Select(x => x.ParameterName))})";
            }

            if (_delete)
            {
                return $"DELETE {context.GetTableName<T>()}";
            }

            throw new InvalidOperationException("Invalid SQL Query");
        }

        private string JoinSql(SqlContext context)
        {
            SqlMap<T> map = context.GetMap<T>();
            return string.Join(" ", map.Joins.Union(_joins).Select(x => x.ToSql(context)));
        }

        private string OrderBySql(SqlContext context)
        {
            return _orderByFields.Count > 0 ?
                $" ORDER BY {string.Join(",", _orderByFields.Select(x => x.ToSql(context)))}"
                : "";
        }

        private string WhereSql(SqlContext context)
        {
            return _conditions.Count > 0 ? $" WHERE {string.Join(" AND ", _conditions.Select(x => x.ToSql(context)))}" : "";
        }
    }
}
