namespace ODK.E2E.Data;

/// <summary>
/// Reads group (Chapter) and membership state directly from the database, for driving the UI (the join
/// URL needs the group's slug) and for asserting outcomes (approval timestamp, membership existence).
/// </summary>
public static class ChapterDataHelper
{
    public static async Task<DateTime?> GetApprovedUtc(Guid chapterId)
    {
        const string sql = "SELECT ApprovedUtc FROM Chapters WHERE ChapterId = @id";

        await using var builder = E2EQueryBuilder
            .Create(sql)
            .AddParameter("@id", chapterId);

        return await builder.ExecuteScalar<DateTime?>();
    }

    public static async Task<DateTime?> GetPublishedUtc(Guid chapterId)
    {
        const string sql = "SELECT PublishedUtc FROM Chapters WHERE ChapterId = @id";

        await using var builder = E2EQueryBuilder
            .Create(sql)
            .AddParameter("@id", chapterId);

        return await builder.ExecuteScalar<DateTime?>();
    }

    public static async Task<string> GetSlug(Guid chapterId)
    {
        const string sql = "SELECT Slug FROM Chapters WHERE ChapterId = @id";

        await using var builder = E2EQueryBuilder
            .Create(sql)
            .AddParameter("@id", chapterId);

        return await builder.ExecuteScalar<string>()
            ?? throw new InvalidOperationException($"No group found with id '{chapterId}'.");
    }

    public static async Task<bool> IsMember(string emailAddress, Guid chapterId)
    {
        const string sql =
            """
            SELECT COUNT(1)
            FROM MemberChapters mc
            INNER JOIN Members m ON m.MemberId = mc.MemberId
            WHERE m.EmailAddress = @email AND mc.ChapterId = @id
            """;

        await using var builder = E2EQueryBuilder
            .Create(sql)
            .AddParameter("@id", chapterId)
            .AddParameter("@email", emailAddress);

        return await builder.ExecuteScalar<int>() > 0;
    }
}