namespace ODK.E2E.Data;

/// <summary>
/// Reads a chapter's event settings (<c>ChapterEventSettings</c>) to assert admin outcomes - the
/// default day of week and start time that drive the pre-populated create-event date.
/// </summary>
public class ChapterEventSettingsDataHelper : DataHelperBase
{
    public ChapterEventSettingsDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>The stored .NET <c>DayOfWeek</c> int (Sunday = 0 ... Saturday = 6), or null if unset.</summary>
    public async Task<int?> GetDefaultDayOfWeek(Guid chapterId)
    {
        const string sql = "SELECT DefaultDayOfWeek FROM ChapterEventSettings WHERE ChapterId = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", chapterId);

        return await builder.ExecuteScalar<int?>();
    }

    public async Task<TimeSpan?> GetDefaultStartTime(Guid chapterId)
    {
        const string sql = "SELECT DefaultStartTime FROM ChapterEventSettings WHERE ChapterId = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", chapterId);

        return await builder.ExecuteScalar<TimeSpan?>();
    }
}
