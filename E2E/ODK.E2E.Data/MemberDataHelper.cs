namespace ODK.E2E.Data;

/// <summary>
/// Reads member records directly from the database - e.g. to resolve a member's id for the member-facing
/// profile page URL (<c>/.../members/{memberId}</c>).
/// </summary>
public class MemberDataHelper : DataHelperBase
{
    public MemberDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<string> GetEmailAddress(Guid memberId)
    {
        const string sql = "SELECT EmailAddress FROM Members WHERE Id = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", memberId);

        return await builder.ExecuteScalar<string>()
            ?? throw new InvalidOperationException($"No member found with id '{memberId}'.");
    }

    public async Task<string?> GetLocale(Guid memberId)
    {
        // MemberPreferences.Locale, mapped to the "Locale" column. Null when the member has no preferences
        // row or no stored locale.
        const string sql = "SELECT Locale FROM MemberPreferences WHERE MemberId = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", memberId);

        return await builder.ExecuteScalar<string>();
    }

    public async Task<Guid> GetMemberId(string emailAddress)
    {
        const string sql = "SELECT Id FROM Members WHERE EmailAddress = @email";

        await using var builder = Builder(sql)
            .AddParameter("@email", emailAddress);

        var memberId = await builder.ExecuteScalar<Guid?>();
        return memberId
            ?? throw new InvalidOperationException($"No member found with email '{emailAddress}'.");
    }

    public async Task<(string FirstName, string LastName)> GetName(Guid memberId)
    {
        const string sql = "SELECT FirstName, LastName FROM Members WHERE Id = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", memberId);

        var rows = await builder.ReadMany(x => (x.GetString(0), x.GetString(1)));
        return rows.Count > 0
            ? rows.First()
            : throw new InvalidOperationException($"No member found with id '{memberId}'.");
    }

    /// <summary>
    /// Sets a member's timezone directly. The app only sets a member's timezone by geocoding their
    /// location (via the geolocation integration), which isn't wired up in the e2e environment - so there's
    /// no UI path to drive. The <c>TimeZoneId</c> property maps to the <c>TimeZone</c> column.
    /// </summary>
    public async Task SetTimeZone(Guid memberId, string timeZoneId)
    {
        const string sql = "UPDATE Members SET TimeZone = @timeZone WHERE Id = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", memberId)
            .AddParameter("@timeZone", timeZoneId);

        await builder.ExecuteNonQuery();
    }
}
