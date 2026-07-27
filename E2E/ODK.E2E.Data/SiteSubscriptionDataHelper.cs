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
    /// <summary>Name prefix for subscriptions a test creates, so teardown can find and remove them.</summary>
    public const string TestNamePrefix = "e2e-sub-";

    public SiteSubscriptionDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>Returns the id of the named subscription on the given platform (PlatformTypeId), or null.</summary>
    public async Task<Guid?> GetId(string name, int platformTypeId)
    {
        const string sql =
            "SELECT SiteSubscriptionId FROM SiteSubscriptions WHERE Name = @name AND PlatformTypeId = @platform";

        await using var builder = Builder(sql)
            .AddParameter("@name", name)
            .AddParameter("@platform", platformTypeId);

        return await builder.ExecuteScalar<Guid?>();
    }

    /// <summary>The Stripe external price id (<c>price_...</c>) of the subscription's cheapest price, or null.</summary>
    public async Task<string?> GetPriceExternalId(Guid subscriptionId)
    {
        const string sql =
            "SELECT TOP 1 ExternalId FROM SiteSubscriptionPrices WHERE SiteSubscriptionId = @id ORDER BY Amount";

        await using var builder = Builder(sql).AddParameter("@id", subscriptionId);
        return await builder.ExecuteScalar<string?>();
    }

    /// <summary>Returns the id of the subscription's cheapest price (for the checkout URL), or null.</summary>
    public async Task<Guid?> GetPriceId(Guid subscriptionId)
    {
        const string sql =
            "SELECT TOP 1 SiteSubscriptionPriceId FROM SiteSubscriptionPrices WHERE SiteSubscriptionId = @id ORDER BY Amount";

        await using var builder = Builder(sql).AddParameter("@id", subscriptionId);
        return await builder.ExecuteScalar<Guid?>();
    }

    /// <summary>Whether the subscription has the given <c>SiteFeatureType</c> value (e.g. 5 = MemberSubscriptions).</summary>
    public async Task<bool> HasFeature(Guid subscriptionId, int featureId)
    {
        const string sql =
            "SELECT COUNT(1) FROM SiteSubscriptionFeatures WHERE SiteSubscriptionId = @id AND SiteFeatureId = @feature";

        await using var builder = Builder(sql)
            .AddParameter("@id", subscriptionId)
            .AddParameter("@feature", featureId);

        return await builder.ExecuteScalar<int>() > 0;
    }

    /// <summary>How many prices the subscription has.</summary>
    public async Task<int> PriceCount(Guid subscriptionId)
    {
        const string sql =
            "SELECT COUNT(1) FROM SiteSubscriptionPrices WHERE SiteSubscriptionId = @id";

        await using var builder = Builder(sql).AddParameter("@id", subscriptionId);
        return await builder.ExecuteScalar<int>();
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
