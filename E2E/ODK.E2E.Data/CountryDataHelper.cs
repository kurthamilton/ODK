namespace ODK.E2E.Data;

/// <summary>
/// Reads and writes country reference data. Used by the site-admin countries tests to assert a saved
/// default locale and to restore the shared reference row afterwards.
/// </summary>
public class CountryDataHelper : DataHelperBase
{
    public CountryDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<string?> GetDefaultLocale(Guid countryId)
    {
        const string sql = "SELECT DefaultLocale FROM Countries WHERE CountryId = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", countryId);

        return await builder.ExecuteScalar<string?>();
    }

    public async Task<Guid> GetFirstCountryId()
    {
        const string sql = "SELECT TOP 1 CountryId FROM Countries ORDER BY Name";

        await using var builder = Builder(sql);

        var id = await builder.ExecuteScalar<Guid?>();
        return id ?? throw new InvalidOperationException("No countries are seeded.");
    }

    public async Task SetDefaultLocale(Guid countryId, string? locale)
    {
        const string sql = "UPDATE Countries SET DefaultLocale = @locale WHERE CountryId = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", countryId)
            .AddParameter("@locale", (object?)locale ?? DBNull.Value);

        await builder.ExecuteNonQuery();
    }
}
