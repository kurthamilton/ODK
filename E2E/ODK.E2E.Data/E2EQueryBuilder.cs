using Microsoft.Data.SqlClient;
using ODK.E2E.Core;

namespace ODK.E2E.Data;

public class E2EQueryBuilder : IAsyncDisposable
{
    private readonly SqlCommand _command;
    private readonly SqlConnection _connection;
    private bool _open;

    private E2EQueryBuilder(SqlConnection connection, SqlCommand command)
    {
        _command = command;
        _connection = connection;
    }

    public async ValueTask DisposeAsync()
    {
        await _command.DisposeAsync();
        await _connection.DisposeAsync();
    }

    internal static E2EQueryBuilder Create(string sql)
    {
        var connection = new SqlConnection(E2ESettings.ConnectionString);
        var command = new SqlCommand(sql, connection);
        return new E2EQueryBuilder(connection, command);
    }

    internal E2EQueryBuilder AddParameter(string name, object value)
    {
        _command.Parameters.AddWithValue(name, value);
        return this;
    }

    internal async Task<int> ExecuteNonQuery()
    {
        await Open();
        return await _command.ExecuteNonQueryAsync();
    }

    internal async Task<T?> ExecuteScalar<T>()
    {
        await Open();
        var result = await _command.ExecuteScalarAsync();

        // ExecuteScalar returns null for an empty result set and DBNull.Value for a SQL NULL value.
        // Call with a nullable type argument (e.g. ExecuteScalar<DateTime?>()) to get null back for a
        // value-type column - with a non-nullable T (ExecuteScalar<DateTime>()) `T?` is just `T`, so a
        // missing value comes back as default(T), not null.
        if (result is null or DBNull)
        {
            return default;
        }

        return (T)result;
    }

    internal async Task<IReadOnlyCollection<T>> ReadMany<T>(Func<SqlDataReader, T> readSingle)
    {
        var results = new List<T>();

        await Open();
        await using var reader = await _command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(readSingle(reader));
        }

        return results;
    }

    private async Task Open()
    {
        if (_open)
        {
            return;
        }

        await _connection.OpenAsync();
        _open = true;
    }
}