namespace ODK.E2E.Data;

/// <summary>
/// Reads a member's chapter-subscription state - the rows the purchase-completion webhook writes. A
/// completed purchase adds a <c>MemberSubscriptionLog</c> row (binding member -> the specific chapter
/// subscription -> payment) and upserts a <c>MemberSubscriptions</c> row (keyed by MemberChapter) whose
/// <c>ExpiresUtc</c> is the active signal. Tests poll these after driving checkout (completion is
/// webhook-driven, so asynchronous).
/// </summary>
public class MemberChapterSubscriptionDataHelper : DataHelperBase
{
    public MemberChapterSubscriptionDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>The member's chapter-subscription expiry (UTC) for the chapter, or null if they have none.</summary>
    public async Task<DateTime?> GetExpiryUtc(Guid memberId, Guid chapterId)
    {
        const string sql =
            """
            SELECT s.ExpiresUtc
            FROM MemberSubscriptions s
            INNER JOIN MemberChapters mc ON mc.MemberChapterId = s.MemberChapterId
            WHERE mc.MemberId = @memberId AND mc.ChapterId = @chapterId
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@chapterId", chapterId);

        return await builder.ExecuteScalar<DateTime?>();
    }

    /// <summary>Whether a completed-purchase record binds the member to the given chapter subscription.</summary>
    public async Task<bool> HasSubscriptionRecord(Guid memberId, Guid chapterSubscriptionId)
    {
        const string sql =
            "SELECT COUNT(1) FROM MemberSubscriptionLog WHERE MemberId = @memberId AND ChapterSubscriptionId = @subscriptionId";

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@subscriptionId", chapterSubscriptionId);

        return await builder.ExecuteScalar<int>() > 0;
    }
}
