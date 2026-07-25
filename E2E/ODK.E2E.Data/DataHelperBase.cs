namespace ODK.E2E.Data;

public abstract class DataHelperBase
{
    private readonly string _connectionString;

    protected DataHelperBase(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected E2EQueryBuilder Builder(string sql)
        => E2EQueryBuilder.Create(_connectionString, sql);
}