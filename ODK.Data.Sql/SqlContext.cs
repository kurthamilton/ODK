using System.Data.Common;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using ODK.Data.Sql.Mapping;
using ODK.Data.Sql.Queries;

namespace ODK.Data.Sql;

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
        var map = GetMap(typeof(T)) as SqlMap<T>;
        if (map == null)
        {
            throw new Exception($"SqlMap for type {typeof(T)} not found");
        }

        return map;
    }

    public string GetTableName<T>()
    {
        SqlMap<T> map = GetMap<T>();
        return map.TableName;
    }

    public string GetTableName(Type type)
    {
        SqlMap map = (SqlMap)GetMap(type);
        return map.TableName;
    }

    public SqlInsertEntityQuery<T> Insert<T>(T entity)
    {
        return new SqlInsertEntityQuery<T>(this, entity);
    }

    public async Task<TRecord?> ReadRecordAsync<T, TRecord>(SqlQuery<T> query, Func<DbDataReader, TRecord> read)
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

    public async Task<T?> ReadRecordAsync<T>(SqlQuery<T> query)
    {
        SqlMap<T> map = GetMap<T>();
        return await ReadRecordAsync(query, map.Read);
    }

    public async Task<IReadOnlyCollection<T>> ReadRecordsAsync<T>(SqlQuery<T> query)
    {
        var records = await ReadQueryAsync(query, reader =>
        {
            List<T> records = new List<T>();

            SqlMap<T> map = GetMap<T>();
            while (reader.Read())
            {
                T record = map.Read(reader);
                records.Add(record);
            }

            return records;
        });

        return records ?? new List<T>();
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
        var key = typeof(T).FullName;
        if (key == null)
        {
            return;
        }

        _maps[key] = map;
    }

    private static string? GetKey(Type type) => type.FullName;

    private async Task ExecuteQueryAsync<T>(SqlQuery<T> query, Func<DbCommand, Task> action, string appendSql = "")
    {
        string sql = query.ToSql(this) + appendSql;

        IEnumerable<(SqlColumn, string?, object?)> parameterValues = query.GetParameterValues(this);

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using DbCommand command = new SqlCommand(sql, connection);
        foreach ((SqlColumn column, string? parameterName, object? value) in parameterValues)
        {
            command.Parameters.Add(column.ToParameter(parameterName, value));
        }

        await connection.OpenAsync();
        await action(command);
    }

    private object GetMap(Type type)
    {
        var key = GetKey(type);
        if (key == null || !_maps.ContainsKey(key))
        {
            throw new InvalidOperationException($"Type {key} not mapped in sql context");
        }
        return _maps[key];
    }

    private async Task<TResult?> ReadQueryAsync<T, TResult>(SqlQuery<T> query, 
        Func<DbDataReader, TResult> read, string appendSql = "") 
    {
        TResult? result = default;

        await ExecuteQueryAsync(query, async command =>
        {
            DbDataReader reader = await command.ExecuteReaderAsync();
            result = read(reader);
        }, appendSql);

        return result;
    }
}
