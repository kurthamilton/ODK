using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Queries
{
    public abstract class SqlQuery<T>
    {
        private readonly IList<IList<ISqlQueryCondition>> _conditions = new List<IList<ISqlQueryCondition>>();
        private readonly IDictionary<SqlColumn, IList<(string, object?)>> _conditionValues = 
            new Dictionary<SqlColumn, IList<(string, object?)>>();
        private bool _delete;
        private int _fetch;
        private T _insertEntity;
        private SqlColumn? _insertOutputColumn;
        private readonly IList<ISqlComponent> _joins = new List<ISqlComponent>();
        private int _offset;
        private readonly IList<ISqlComponent> _orderByFields = new List<ISqlComponent>();
        private readonly IList<string> _selectColumns = new List<string>();
        private int _top;
        private readonly IList<(SqlColumn Column, object Value)> _updateColumns = new List<(SqlColumn, object)>();

        protected SqlQuery(SqlContext context)
        {
            Context = context;
        }

        protected SqlContext Context { get; }

        public string AddParameterValue<TEntity, TValue>(Expression<Func<TEntity, TValue>> field, TValue value)
        {
            SqlColumn? column = Context.GetColumn(field);
            if (column == null)
            {
                return "";
            }

            if (!_conditionValues.ContainsKey(column))
            {
                _conditionValues.Add(column, new List<(string, object?)>());
            }

            string parameterName = $"{column.ParameterName}{_conditionValues[column].Count}";

            _conditionValues[column].Add((parameterName, value));
            return parameterName;
        }

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

        public IEnumerable<(SqlColumn, string?, object?)> GetParameterValues(SqlContext context)
        {
            foreach (SqlColumn column in _conditionValues.Keys)
            {
                foreach ((string parameterName, object? parameterValue) in _conditionValues[column])
                {
                    yield return (column, parameterName, parameterValue);
                }
            }

            if (_insertEntity != null)
            {
                SqlMap<T> map = context.GetMap<T>();

                foreach (SqlColumn column in map.InsertColumns)
                {
                    object? value = map.GetEntityValue(_insertEntity, column, context);

                    yield return (column, null, value);
                }
            }

            if (_updateColumns.Count > 0)
            {
                foreach ((SqlColumn column, object value) in _updateColumns)
                {
                    yield return (column, null, value);
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
               OrderBySql(context) +
               PageSql();
        }

        public async Task<int> VersionAsync()
        {
            string columnSql = string.Join(",", _selectColumns);
            _selectColumns.Clear();
            AddSelectColumn($"ISNULL(CHECKSUM_AGG(CHECKSUM({columnSql})),0)");
            return await Context.ReadRecordAsync(this, reader => reader.GetInt32(0));
        }

        protected void AddCondition<TEntity, TValue, TQuery>(SqlQueryCondition<T, TEntity, TValue, TQuery> condition) where TQuery : SqlQuery<T>
        {
            AddConditions(new [] { condition });
        }

        protected void AddConditions<TEntity, TValue, TQuery>(IEnumerable<SqlQueryCondition<T, TEntity, TValue, TQuery>> conditions) where TQuery : SqlQuery<T>
        {
            _conditions.Add(new List<ISqlQueryCondition>(conditions));
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

        protected void AddJoin<TFrom, TTo, TValue>(Expression<Func<TFrom, TValue>> fromField, Expression<Func<TTo, TValue>> toField, SqlJoinType type = SqlJoinType.Inner)
        {
            SqlJoin<TFrom, TTo, TValue> join = new SqlJoin<TFrom, TTo, TValue>(fromField, toField, type);
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

        protected void AddPage(int page, int pageSize)
        {
            if (pageSize < 1)
            {
                return;
            }

            _fetch = pageSize;
            _offset = pageSize * (page - 1);
        }

        protected void AddTop(int size)
        {
            _top = size;
        }

        protected void AddUpdateColumn<TValue>(Expression<Func<T, TValue>> expression, TValue value)
        {
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
                return $"SELECT {(_top > 0 ? $"TOP {_top} " : "")}{string.Join(",", _selectColumns)}";
            }

            if (_updateColumns.Count > 0)
            {
                return $"UPDATE {context.GetTableName<T>()} " +
                       $"SET {string.Join(",", _updateColumns.Select(x => $"{x.Column.ToSql(context)} = {x.Column.ParameterName}"))}";
            }

            if (_insertEntity != null)
            {
                SqlMap<T> map = context.GetMap<T>();
                IReadOnlyCollection<SqlColumn> columns = map.InsertColumns;
                return $"INSERT INTO {context.GetTableName<T>()} ({string.Join(",", columns.Select(x => $"[{x.ColumnName}]"))}) " +
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
            if (_insertEntity != null)
            {
                return "";
            }

            SqlMap<T> map = context.GetMap<T>();
            return string.Join(" ", map.Joins.Union(_joins).Select(x => x.ToSql(context)));
        }

        private string OrderBySql(SqlContext context)
        {
            return _orderByFields.Count > 0 ?
                $" ORDER BY {string.Join(",", _orderByFields.Select(x => x.ToSql(context)))}"
                : "";
        }

        private string PageSql()
        {
            if (_fetch == 0)
            {
                return "";
            }

            return $" OFFSET {_offset} ROWS FETCH NEXT {_fetch} ROWS ONLY";
        }

        private string WhereSql(SqlContext context)
        {
            if (_conditions.Count == 0)
            {
                return "";
            }

            StringBuilder sql = new StringBuilder(" WHERE ");

            for (int i = 0; i < _conditions.Count; i++)
            {
                IList<ISqlQueryCondition> group = _conditions[i];

                if (i > 0)
                {
                    sql.Append(" AND ");
                }

                sql.Append("(");
                sql.Append(string.Join(" OR ", group.Select(x => x.ToSql(context))));
                sql.Append(")");
            }

            return sql.ToString();
        }
    }
}
