using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
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

        public SqlMap<T> GetMap<T>()
        {
            string key = typeof(T).FullName;
            if (!_maps.ContainsKey(key))
            {
                throw new InvalidOperationException($"Type {key} not mapped in sql context");
            }
            return _maps[key] as SqlMap<T>;
        }

        public async Task<Guid> InsertAsync<T>(T entity)
        {
            SqlQuery<T> query = new SqlInsertValuesQuery<T>(this)
                .Value(entity)
                .OutputIdentity();

            return await ReadRecordAsync(query, reader => reader.GetGuid(0));
        }

        public async Task<TRecord> ReadRecordAsync<T, TRecord>(SqlQuery<T> query, Func<DbDataReader, TRecord> read)
        {
            return await ReadQueryAsync(query, reader =>
            {
                if (!reader.Read())
                {
                    return default(TRecord);
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

        private async Task ExecuteQueryAsync<T>(SqlQuery<T> query, Func<DbCommand, Task> action, string appendSql = "")
        {
            string sql = query.ToSql() + appendSql;

            IEnumerable<(SqlColumn, object)> parameterValues = query.GetParameterValues();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (DbCommand command = new SqlCommand(sql, connection))
                {
                    foreach ((SqlColumn column, object value) in parameterValues)
                    {
                        command.Parameters.Add(column.ToParameter(value));
                    }

                    await connection.OpenAsync();
                    await action(command);
                }
            }
        }

        private async Task<TResult> ReadQueryAsync<T, TResult>(SqlQuery<T> query, Func<DbDataReader, TResult> read, string appendSql = "")
        {
            TResult result = default(TResult);

            await ExecuteQueryAsync(query, async command =>
            {
                DbDataReader reader = await command.ExecuteReaderAsync();
                result = read(reader);
            }, appendSql);

            return result;
        }
    }
}
