namespace ODK.E2E.Data;

/// <summary>
/// Reads a member's site subscription - the row the payment-completion webhook writes. A purchase is
/// complete once <c>MemberSiteSubscriptions.ExpiresUtc</c> is a future date; a test polls this after
/// driving checkout (completion is webhook-driven, so asynchronous).
/// </summary>
public class MemberSiteSubscriptionDataHelper : DataHelperBase
{
    public MemberSiteSubscriptionDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>
    /// Upserts an active site subscription for the member (expiring a year out), pointing at the given
    /// subscription + price - so the member (e.g. a chapter owner) gains that subscription's features
    /// without a real purchase. One row per member (the entity is one-to-one with Member).
    /// </summary>
    public async Task EnsureActive(Guid memberId, Guid siteSubscriptionId, Guid siteSubscriptionPriceId)
    {
        const string sql =
            """
            IF NOT EXISTS (SELECT 1 FROM MemberSiteSubscriptions WHERE MemberId = @memberId)
                INSERT INTO MemberSiteSubscriptions
                    (MemberSiteSubscriptionId, MemberId, SiteSubscriptionId, SiteSubscriptionPriceId, ExpiresUtc)
                VALUES (NEWID(), @memberId, @subId, @priceId, @expires);
            ELSE
                UPDATE MemberSiteSubscriptions
                SET SiteSubscriptionId = @subId, SiteSubscriptionPriceId = @priceId, ExpiresUtc = @expires
                WHERE MemberId = @memberId;
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@subId", siteSubscriptionId)
            .AddParameter("@priceId", siteSubscriptionPriceId)
            .AddParameter("@expires", DateTime.UtcNow.AddYears(1));

        await builder.ExecuteNonQuery();
    }

    /// <summary>The member's site-subscription expiry (UTC), or null if they have no site subscription yet.</summary>
    public async Task<DateTime?> GetExpiresUtc(Guid memberId)
    {
        const string sql = "SELECT ExpiresUtc FROM MemberSiteSubscriptions WHERE MemberId = @memberId";

        await using var builder = Builder(sql).AddParameter("@memberId", memberId);
        return await builder.ExecuteScalar<DateTime?>();
    }
}
