namespace ODK.E2E.Data;

/// <summary>
/// Direct-to-DB DrunkenKnitwits site-subscription setup. Creating a DrunkenKnitwits chapter account
/// (the join flow, <c>MemberService.CreateChapterAccount</c>) assigns the new member the platform's
/// default site subscription, loaded via <c>SiteSubscriptionRepository.GetDefault</c> - a throwing
/// single query that requires an <c>Enabled</c>, <c>Default</c> row for the platform. Subscriptions are
/// created through the UI (which also creates the Stripe product); this makes the one the tests created
/// the platform's default.
/// </summary>
public class SiteSubscriptionDataHelper : DataHelperBase
{
    public SiteSubscriptionDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>
    /// Makes the named DrunkenKnitwits subscription the platform's sole default, so
    /// <c>GetDefault(DrunkenKnitwits)</c> (which requires an enabled, default row) returns it.
    /// </summary>
    public async Task SetDrunkenKnitwitsDefault(string name)
    {
        const string sql =
            "UPDATE SiteSubscriptions SET [Default] = CASE WHEN Name = @name THEN 1 ELSE 0 END WHERE PlatformTypeId = 2";

        await using var builder = Builder(sql).AddParameter("@name", name);
        await builder.ExecuteNonQuery();
    }
}
