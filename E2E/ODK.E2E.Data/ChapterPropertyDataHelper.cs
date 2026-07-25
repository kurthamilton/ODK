namespace ODK.E2E.Data;

/// <summary>
/// Reads chapter member-profile properties (<c>ChapterProperties</c>) - the per-chapter questions
/// members answer. Used to resolve a property's id from its label (the join/profile forms key fields by
/// id) and to assert its display order after a reorder.
/// </summary>
public class ChapterPropertyDataHelper : DataHelperBase
{
    public ChapterPropertyDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<int?> GetDisplayOrder(Guid chapterPropertyId)
    {
        const string sql = "SELECT DisplayOrder FROM ChapterProperties WHERE ChapterPropertyId = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", chapterPropertyId);

        return await builder.ExecuteScalar<int?>();
    }

    public async Task<Guid?> GetPropertyId(Guid chapterId, string label)
    {
        const string sql =
            "SELECT TOP 1 ChapterPropertyId FROM ChapterProperties WHERE ChapterId = @chapterId AND Label = @label";

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@label", label);

        return await builder.ExecuteScalar<Guid?>();
    }
}
