using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ODK.Data.Sql.Mapping;
using ODK.Data.Sql.Queries;

namespace ODK.Data.Sql
{
    public abstract class SqlContext
    {
        private readonly string _connectionString;
        private readonly IDictionary<string, object> _maps = new Dictionary<string, object>();

        protected SqlContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlDeleteQuery<T> Delete<T>()
        {
            return new SqlDeleteQuery<T>(this);
        }

        public async Task ExecuteNonQueryAsync<T>(SqlQuery<T> query)
        {
            await ExecuteQueryAsync(query, command => command.ExecuteNonQueryAsync());
        }

        public SqlColumn GetColumn<T, TValue>(Expression<Func<T, TValue>> expression)
        {
            SqlMap<T> map = GetMap<T>();
            return map.GetColumn(expression);
        }

        public SqlMap<T> GetMap<T>()
        {
            string key = GetKey<T>();
            if (!_maps.ContainsKey(key))
            {
                throw new InvalidOperationException($"Type {key} not mapped in sql context");
            }
            return _maps[key] as SqlMap<T>;
        }

        public string GetTableName<T>()
        {
            SqlMap<T> map = GetMap<T>();
            return map.TableName;
        }

        public SqlInsertEntityQuery<T> Insert<T>(T entity)
        {
            return new SqlInsertEntityQuery<T>(this, entity);
        }

        public async Task<TRecord> ReadRecordAsync<T, TRecord>(SqlQuery<T> query, Func<DbDataReader, TRecord> read)
        {
            return await ReadQueryAsync(query, reader =>
            {
                if (!reader.Read())
                {
                    return default;
                }

                return read(reader);
            });
        }

        public async Task<T> ReadRecordAsync<T>(SqlQuery<T> query)
        {
            SqlMap<T> map = GetMap<T>();
            return await ReadRecordAsync(query, map.Read);
        }

        public async Task<IReadOnlyCollection<T>> ReadRecordsAsync<T>(SqlQuery<T> query)
        {
            return await ReadQueryAsync(query, reader =>
            {
                SqlMap<T> map = GetMap<T>();
                List<T> records = new List<T>();
                while (reader.Read())
                {
                    T record = map.Read(reader);
                    records.Add(record);
                }

                return records;
            });
        }

        public SqlSelectQuery<T> Select<T>()
        {
            return new SqlSelectQuery<T>(this);
        }

        public SqlUpdateQuery<T> Update<T>()
        {
            return new SqlUpdateQuery<T>(this);
        }

        protected void AddMap<T>(SqlMap<T> map)
        {
            string key = typeof(T).FullName;
            if (key == null)
            {
                return;
            }

            _maps[key] = map;
        }

        private static string GetKey<T>()
        {
            return typeof(T).FullName;
        }

        private async Task ExecuteQueryAsync<T>(SqlQuery<T> query, Func<DbCommand, Task> action, string appendSql = "")
        {
            string sql = query.ToSql(this) + appendSql;

            IEnumerable<(SqlColumn, object)> parameterValues = query.GetParameterValues(this);

            await using SqlConnection connection = new SqlConnection(_connectionString);
            await using DbCommand command = new SqlCommand(sql, connection);
            foreach ((SqlColumn column, object value) in parameterValues)
            {
                command.Parameters.Add(column.ToParameter(value));
            }

            await connection.OpenAsync();
            await action(command);
        }

        private async Task<TResult> ReadQueryAsync<T, TResult>(SqlQuery<T> query, Func<DbDataReader, TResult> read, string appendSql = "")
        {
            TResult result = default;

            await ExecuteQueryAsync(query, async command =>
            {
                DbDataReader reader = await command.ExecuteReaderAsync();
                result = read(reader);
            }, appendSql);

            return result;
        }
    }
}
