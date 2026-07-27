namespace ODK.E2E.Data;

/// <summary>
/// Reads chapter subscriptions (<c>ChapterSubscriptions</c>) - a single table carrying the amount,
/// recurring flag, and Stripe external ids directly (no separate price table) - to verify the admin
/// create flow. A subscription is identified by chapter + name (the system identifier on the form).
/// </summary>
public class ChapterSubscriptionDataHelper : DataHelperBase
{
    public ChapterSubscriptionDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>The Stripe external price id created for the named subscription, or null if none/absent.</summary>
    public async Task<string?> GetExternalId(Guid chapterId, string name)
    {
        const string sql =
            "SELECT ExternalId FROM ChapterSubscriptions WHERE ChapterId = @chapterId AND Name = @name";

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@name", name);

        return await builder.ExecuteScalar<string?>();
    }

    /// <summary>The id of the named subscription (for the member checkout URL), or null if absent.</summary>
    public async Task<Guid?> GetId(Guid chapterId, string name)
    {
        const string sql =
            "SELECT ChapterSubscriptionId FROM ChapterSubscriptions WHERE ChapterId = @chapterId AND Name = @name";

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@name", name);

        return await builder.ExecuteScalar<Guid?>();
    }

    /// <summary>Whether the named subscription is recurring; null if it doesn't exist.</summary>
    public async Task<bool?> IsRecurring(Guid chapterId, string name)
    {
        const string sql =
            "SELECT Recurring FROM ChapterSubscriptions WHERE ChapterId = @chapterId AND Name = @name";

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@name", name);

        return await builder.ExecuteScalar<bool?>();
    }
}
