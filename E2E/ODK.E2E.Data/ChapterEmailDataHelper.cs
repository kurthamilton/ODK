namespace ODK.E2E.Data;

/// <summary>
/// Reads a group's email overrides from <c>ChapterEmails</c>. Subject and body are overridden
/// independently: a null column means the group has not overridden that field, so the send falls back to
/// the site's. A row overriding neither is not kept, so the row count is how a test tells "back on the
/// default" from "customised with the same wording".
/// <para>
/// Keyed on the chapter alone rather than on a chapter and an email type: a test picks whichever template
/// the app offers and customises only that one, so the group has at most a single row.
/// </para>
/// </summary>
public class ChapterEmailDataHelper : DataHelperBase
{
    public ChapterEmailDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>The group's overridden body, or null where it has not overridden one.</summary>
    public async Task<string?> GetBodyHtml(Guid chapterId)
    {
        const string sql = "SELECT BodyHtml FROM ChapterEmails WHERE ChapterId = @chapterId";

        await using var builder = Builder(sql).AddParameter("@chapterId", chapterId);
        return await builder.ExecuteScalar<string?>();
    }

    /// <summary>How many templates the group overrides: 1 once it customises anything, 0 otherwise.</summary>
    public async Task<int> GetRowCount(Guid chapterId)
    {
        const string sql = "SELECT COUNT(1) FROM ChapterEmails WHERE ChapterId = @chapterId";

        await using var builder = Builder(sql).AddParameter("@chapterId", chapterId);
        return await builder.ExecuteScalar<int>();
    }

    /// <summary>The group's overridden subject, or null where it has not overridden one.</summary>
    public async Task<string?> GetSubject(Guid chapterId)
    {
        const string sql = "SELECT Subject FROM ChapterEmails WHERE ChapterId = @chapterId";

        await using var builder = Builder(sql).AddParameter("@chapterId", chapterId);
        return await builder.ExecuteScalar<string?>();
    }
}
