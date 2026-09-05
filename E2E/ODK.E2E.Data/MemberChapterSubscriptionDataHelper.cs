namespace ODK.E2E.Data;

/// <summary>
/// Reads a member's chapter-subscription state from <c>MemberSubscriptionLog</c> (the source of truth). A
/// completed purchase appends a log row (binding member -> the specific chapter subscription -> payment) and
/// flags it current; its <c>ExpiresUtc</c> is the active signal. Tests poll these after driving checkout
/// (completion is webhook-driven, so asynchronous).
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
            SELECT l.ExpiresUtc
            FROM MemberSubscriptionLog l
            WHERE l.MemberId = @memberId AND l.ChapterId = @chapterId AND l.IsCurrent = 1
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@chapterId", chapterId);

        return await builder.ExecuteScalar<DateTime?>();
    }

    /// <summary>
    /// The external (payment provider) id stored on the member's current subscription record for the chapter,
    /// or null if none. Only recurring subscriptions persist one (the Stripe subscription id); one-off
    /// purchases leave it null so no lookup is attempted against a payment-intent id.
    /// </summary>
    public async Task<string?> GetCurrentExternalId(Guid memberId, Guid chapterId)
    {
        const string sql =
            """
            SELECT l.ExternalId
            FROM MemberSubscriptionLog l
            WHERE l.MemberId = @memberId AND l.ChapterId = @chapterId AND l.IsCurrent = 1
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@chapterId", chapterId);

        return await builder.ExecuteScalar<string?>();
    }

    /// <summary>
    /// The payment the member's current subscription record for the chapter was recorded against. Each
    /// billing records itself against its own payment, so on a renewed subscription this is the renewal's -
    /// which is what a test about the renewal's money has to start from.
    /// </summary>
    public async Task<Guid?> GetCurrentPaymentId(Guid memberId, Guid chapterId)
    {
        const string sql =
            """
            SELECT l.PaymentId
            FROM MemberSubscriptionLog l
            WHERE l.MemberId = @memberId AND l.ChapterId = @chapterId AND l.IsCurrent = 1
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@chapterId", chapterId);

        return await builder.ExecuteScalar<Guid?>();
    }

    /// <summary>
    /// How many log rows the member has for the chapter. One row is appended per billing event, so this is
    /// what distinguishes a renewal being applied once from being applied twice - the expiry cannot, since a
    /// recurring subscription's expiry is the provider's next payment date and re-applying an event sets the
    /// same value.
    /// </summary>
    public async Task<int> GetRecordCount(Guid memberId, Guid chapterId)
    {
        const string sql =
            "SELECT COUNT(1) FROM MemberSubscriptionLog WHERE MemberId = @memberId AND ChapterId = @chapterId";

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@chapterId", chapterId);

        return await builder.ExecuteScalar<int>();
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
