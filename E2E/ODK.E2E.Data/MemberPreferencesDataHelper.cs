namespace ODK.E2E.Data;

/// <summary>
/// Sets a member's formatting preferences straight in the database. There's no settings UI yet, so tests
/// seed the locale directly to exercise locale-driven display (e.g. the date picker).
/// </summary>
public class MemberPreferencesDataHelper : DataHelperBase
{
    public MemberPreferencesDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task SetLocale(Guid memberId, string locale)
    {
        const string sql =
            """
            IF EXISTS (SELECT 1 FROM MemberPreferences WHERE MemberId = @memberId)
                UPDATE MemberPreferences SET Locale = @locale WHERE MemberId = @memberId;
            ELSE
                INSERT INTO MemberPreferences (MemberId, Locale) VALUES (@memberId, @locale);
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@locale", locale);

        await builder.ExecuteNonQuery();
    }
}
