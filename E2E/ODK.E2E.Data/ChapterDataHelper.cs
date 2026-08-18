namespace ODK.E2E.Data;

/// <summary>
/// Reads group (Chapter) and membership state directly from the database, for driving the UI (the join
/// URL needs the group's slug) and for asserting outcomes (approval timestamp, membership existence).
/// </summary>
public class ChapterDataHelper : DataHelperBase
{
    public ChapterDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<DateTime?> GetApprovedUtc(Guid chapterId)
    {
        const string sql = "SELECT ApprovedUtc FROM Chapters WHERE Id = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", chapterId);

        return await builder.ExecuteScalar<DateTime?>();
    }

    public async Task<DateTime?> GetPublishedUtc(Guid chapterId)
    {
        const string sql = "SELECT PublishedUtc FROM Chapters WHERE Id = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", chapterId);

        return await builder.ExecuteScalar<DateTime?>();
    }

    public async Task<string> GetSlug(Guid chapterId)
    {
        const string sql = "SELECT Slug FROM Chapters WHERE Id = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", chapterId);

        return await builder.ExecuteScalar<string>()
            ?? throw new InvalidOperationException($"No group found with id '{chapterId}'.");
    }

    public async Task<string> GetTimeZoneId(Guid chapterId)
    {
        // The TimeZoneId property maps to the "TimeZone" column.
        const string sql = "SELECT TimeZone FROM Chapters WHERE Id = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", chapterId);

        return await builder.ExecuteScalar<string>()
            ?? throw new InvalidOperationException($"No group found with id '{chapterId}'.");
    }

    /// <summary>
    /// Re-platforms an existing chapter to DrunkenKnitwits and sets its approval/publish state. There
    /// is no self-service DrunkenKnitwits chapter creation, so tests create a valid chapter through the
    /// Default UI (which writes all the dependent rows) and flip it here - the minimal DB change needed
    /// to get a usable DrunkenKnitwits chapter. Extra data (questions, properties, subscriptions) is
    /// seeded on demand by the tests that need it.
    /// </summary>
    public async Task SetDrunkenKnitwitsChapter(Guid chapterId, bool approved, bool published)
    {
        // DrunkenKnitwits resolves a chapter from its URL by appending the " Drunken Knitwits" suffix to
        // the URL segment and matching on Name (the segment is the ShortName = Name minus the suffix).
        // The chapter was created on Default with an un-suffixed name, so append the suffix here (guarded
        // so it isn't doubled) - otherwise the DrunkenKnitwits URL can't find the chapter (404).
        const string sql =
            """
            UPDATE Chapters
            SET PlatformTypeId = 2,
                Name = CASE WHEN Name LIKE '% Drunken Knitwits' THEN Name ELSE Name + ' Drunken Knitwits' END,
                ApprovedUtc = @approvedUtc,
                PublishedUtc = @publishedUtc
            WHERE Id = @id
            """;

        await using var builder = Builder(sql)
            .AddParameter("@id", chapterId)
            .AddParameter("@approvedUtc", approved ? DateTime.UtcNow : (object)DBNull.Value)
            .AddParameter("@publishedUtc", published ? DateTime.UtcNow : (object)DBNull.Value);

        await builder.ExecuteNonQuery();
    }

    /// <summary>
    /// Whether the address has an <em>approved</em> membership of the group. Distinct from
    /// <see cref="IsMember"/>: a member of a group that vets new members has a row from the moment they
    /// apply, and approval is a flag on it.
    /// </summary>
    public async Task<bool> IsApprovedMember(string emailAddress, Guid chapterId)
    {
        const string sql =
            """
            SELECT COUNT(1)
            FROM MemberChapters mc
            INNER JOIN Members m ON m.Id = mc.MemberId
            WHERE m.EmailAddress = @email AND mc.ChapterId = @id AND mc.Approved = 1
            """;

        await using var builder = Builder(sql)
            .AddParameter("@id", chapterId)
            .AddParameter("@email", emailAddress);

        return await builder.ExecuteScalar<int>() > 0;
    }

    public async Task<bool> IsMember(string emailAddress, Guid chapterId)
    {
        const string sql =
            """
            SELECT COUNT(1)
            FROM MemberChapters mc
            INNER JOIN Members m ON m.Id = mc.MemberId
            WHERE m.EmailAddress = @email AND mc.ChapterId = @id
            """;

        await using var builder = Builder(sql)
            .AddParameter("@id", chapterId)
            .AddParameter("@email", emailAddress);

        return await builder.ExecuteScalar<int>() > 0;
    }
}