namespace ODK.E2E.Data;

/// <summary>
/// Reads venue state directly from the database to assert admin outcomes (a venue was created for a
/// chapter) and to drive later flows that need one.
/// <para>
/// A venue is site-level and a group reaches it through a ChapterVenue, which is what the group's own
/// name for it lives on and what its admin pages and the event form are keyed by - so what a test needs
/// is nearly always the link's id rather than the venue's. The slug is the venue's, being the place's.
/// </para>
/// </summary>
public class VenueDataHelper : DataHelperBase
{
    public VenueDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<Guid?> GetChapterVenueId(Guid chapterId, string name)
    {
        const string sql =
            """
            SELECT TOP 1 cv.Id
            FROM Venues v
            INNER JOIN ChapterVenues cv ON cv.VenueId = v.Id
            WHERE cv.ChapterId = @chapterId
                AND ISNULL(cv.Name, v.Name) = @name
                AND cv.ArchivedUtc IS NULL
            """;

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@name", name);

        return await builder.ExecuteScalar<Guid?>();
    }

    /// <summary>
    /// The venue's own name, which is the place's - as against the name the group gave it, which is what
    /// <paramref name="name"/> finds it by.
    /// </summary>
    public async Task<string?> GetVenueName(Guid chapterId, string name)
    {
        const string sql =
            """
            SELECT TOP 1 v.Name
            FROM Venues v
            INNER JOIN ChapterVenues cv ON cv.VenueId = v.Id
            WHERE cv.ChapterId = @chapterId
                AND ISNULL(cv.Name, v.Name) = @name
                AND cv.ArchivedUtc IS NULL
            """;

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@name", name);

        return await builder.ExecuteScalar<string>();
    }

    /// <summary>
    /// Which venue the group's link points at. Two groups holding the same id is the whole of what venue
    /// reuse means - and the only way to see it, since reuse is deliberately invisible to a group.
    /// </summary>
    public async Task<Guid?> GetVenueIdForChapter(Guid chapterId, string name)
    {
        const string sql =
            """
            SELECT TOP 1 cv.VenueId
            FROM Venues v
            INNER JOIN ChapterVenues cv ON cv.VenueId = v.Id
            WHERE cv.ChapterId = @chapterId
                AND ISNULL(cv.Name, v.Name) = @name
                AND cv.ArchivedUtc IS NULL
            """;

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@name", name);

        return await builder.ExecuteScalar<Guid?>();
    }

    /// <summary>
    /// The venue's slug. Null both when no such venue exists and when the venue has no slug, which is
    /// indistinguishable here - assert existence separately if the difference matters.
    /// </summary>
    public async Task<string?> GetVenueSlug(Guid chapterId, string name)
    {
        const string sql =
            """
            SELECT TOP 1 v.Slug
            FROM Venues v
            INNER JOIN ChapterVenues cv ON cv.VenueId = v.Id
            WHERE cv.ChapterId = @chapterId
                AND ISNULL(cv.Name, v.Name) = @name
                AND cv.ArchivedUtc IS NULL
            """;

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@name", name);

        return await builder.ExecuteScalar<string>();
    }

    public async Task<bool> VenueExists(Guid chapterId, string name)
        => await GetChapterVenueId(chapterId, name) is not null;
}
