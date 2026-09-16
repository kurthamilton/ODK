namespace ODK.E2E.Data;

/// <summary>
/// Reads venue state directly from the database to assert admin outcomes (a venue was created for a
/// chapter) and to drive later flows that need a venue's id.
/// </summary>
public class VenueDataHelper : DataHelperBase
{
    public VenueDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<Guid?> GetVenueId(Guid chapterId, string name)
    {
        const string sql =
            """
            SELECT TOP 1 v.Id
            FROM Venues v
            INNER JOIN ChapterVenues cv ON cv.VenueId = v.Id
            WHERE cv.ChapterId = @chapterId AND v.Name = @name AND cv.ArchivedUtc IS NULL
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
            WHERE cv.ChapterId = @chapterId AND v.Name = @name AND cv.ArchivedUtc IS NULL
            """;

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@name", name);

        return await builder.ExecuteScalar<string>();
    }

    public async Task<bool> VenueExists(Guid chapterId, string name)
        => await GetVenueId(chapterId, name) is not null;
}
