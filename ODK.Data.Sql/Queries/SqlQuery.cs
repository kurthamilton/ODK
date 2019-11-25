using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Queries
{
    public abstract class SqlQuery<T>
    {
        private readonly IList<string> _conditions = new List<string>();
        private string _from = "";
        private readonly IList<ISqlComponent> _joins = new List<ISqlComponent>();
        private readonly IList<string> _orderByFields = new List<string>();
        private readonly IList<SqlColumn> _updateColumns = new List<SqlColumn>();

        private readonly StringBuilder _sql = new StringBuilder();

        protected SqlQuery(SqlContext context)
        {
            Context = context;
        }

        protected IDictionary<string, SqlColumn> Columns { get; } = new Dictionary<string, SqlColumn>();

        protected SqlContext Context { get; }

        protected IDictionary<string, object> ParameterValues { get; } = new Dictionary<string, object>();

        public async Task ExecuteAsync()
        {
            await Context.ExecuteNonQueryAsync(this);
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            return await Context.ReadRecordAsync(this);
        }

        public async Task<IReadOnlyCollection<T>> ToArrayAsync()
        {
            return await Context.ReadRecordsAsync(this);
        }

        public DbCommand ToCommand()
        {
            string sql = ToSql();

            DbCommand command = new SqlCommand(sql.ToString());
            foreach (IDataParameter parameter in GetParameters())
            {
                command.Parameters.Add(parameter);
            }

            return command;
        }

        protected void AddCondition<TEntity, TValue>(Expression<Func<TEntity, TValue>> expression, string @operator, TValue value)
        {
            SqlMap<TEntity> map = Context.GetMap<TEntity>();
            SqlColumn column = map.GetColumn(expression);

            _conditions.Append($"{column.ToSql()} {@operator} {column.ParameterName}");

            AddParameterValue(column, value);
        }

        protected void AddFrom()
        {
            SqlMap<T> map = Context.GetMap<T>();
            _from = map.TableName;
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

        protected void AddParameterValue(SqlColumn column, object value)
        {
            Columns.Add(column.ParameterName, column);
            ParameterValues.Add(column.ParameterName, value);
        }

        protected void AddUpdateColumn<TValue>(Expression<Func<T, TValue>> expression, TValue value)
        {
            SqlMap<T> map = Context.GetMap<T>();

            // TODO: resolve conflict between set parameter and where parameter when both operate on same column
            SqlColumn column = map.GetColumn(expression);
            AddParameterValue(column, value);
            _updateColumns.Add(column);
        }

        protected SqlQuery<T> AppendSql(string sql)
        {
            _sql.Append(sql);
            return this;
        }

        protected string ToSql()
        {
            return _sql.ToString() + 
                UpdateSql() + 
                FromSql() + 
                JoinSql() + 
                WhereSql() + 
                OrderBySql();
        }

        private string FromSql()
        {
            return _from.Length > 0 ? $" FROM {_from}" : "";
        }

        private IEnumerable<IDataParameter> GetParameters()
        {
            foreach (string parameterName in Columns.Keys)
            {
                SqlColumn column = Columns[parameterName];
                object value = ParameterValues[parameterName];

                IDataParameter parameter = column.ToParameter(value);
                yield return parameter;
            }
        }

        private string JoinSql()
        {
            SqlMap<T> map = Context.GetMap<T>();
            return string.Join(" ", map.Joins.Union(_joins).Select(x => x.ToSql(Context)));
        }

        private string OrderBySql()
        {
            return _orderByFields.Count > 0 ? $" ORDER BY {string.Join(",", _orderByFields)}" : "";
        }

        private static string UpdateColumnSql(SqlColumn column)
        {
            return $"{column.ToSql()} = {column.ParameterName}";
        }

        private string UpdateSql()
        {
            if (_updateColumns.Count == 0)
            {
                return "";
            }

            StringBuilder sql = new StringBuilder();

            sql.Append(" SET ");

            sql.Append(string.Join(",", _updateColumns.Select(UpdateColumnSql)));
            
            return sql.ToString();
        }

        private string WhereSql()
        {
            return _conditions.Count > 0 ? $" WHERE {string.Join(" AND ", _conditions)}" : "";
        }
    }
}
