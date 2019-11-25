using System;
using System.Collections.Generic;
using System.Data;
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
            using (DbConnection connection = await OpenConnectionAsync())
            {
                using (DbCommand command = query.ToCommand())
                {
                    command.Connection = connection;

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<T> ExecuteSqlAsync<T>(string sql, Func<DbDataReader, T> read, IEnumerable<IDataParameter> parameters = null)
        {
            using (DbConnection connection = await OpenConnectionAsync())
            {
                using (DbCommand command = new SqlCommand(sql))
                {
                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                        {
                            return default(T);
                        }
                        
                        return read(reader);
                    }
                }
            }
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

        public async Task InsertAsync<T>(T entity)
        {
            SqlQuery<T> query = new SqlInsertValuesQuery<T>(this)
                .Value(entity);
            await ExecuteNonQueryAsync(query);
        }

        public async Task<T> ReadRecordAsync<T>(SqlQuery<T> query)
        {
            using (DbConnection connection = await OpenConnectionAsync())
            {
                using (DbCommand command = query.ToCommand())
                {
                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                        {
                            return default(T);
                        }

                        SqlMap<T> map = GetMap<T>();
                        return map.Read(reader);
                    }
                }
            }
        }

        public async Task<IReadOnlyCollection<T>> ReadRecordsAsync<T>(SqlQuery<T> query)
        {
            List<T> records = new List<T>();

            using (DbConnection connection = await OpenConnectionAsync())
            {
                using (DbCommand command = query.ToCommand())
                {
                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        SqlMap<T> map = GetMap<T>();
                        while (reader.Read())
                        {
                            T record = map.Read(reader);
                            records.Add(record);
                        }

                        return records;
                    }
                }
            }
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
            _maps.Add(key, map);
        }

        private async Task<DbConnection> OpenConnectionAsync()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
