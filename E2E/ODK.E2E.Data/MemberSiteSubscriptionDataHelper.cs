namespace ODK.E2E.Data;

/// <summary>
/// Reads a member's site subscription from the current <c>MemberSiteSubscriptionLog</c> record - the row
/// the payment-completion webhook writes. A purchase is complete once its <c>ExpiresUtc</c> is a future
/// date; a test polls this after driving checkout (completion is webhook-driven, so asynchronous).
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
    /// without a real purchase. The member already has a current MemberSiteSubscriptionLog record (a free
    /// placeholder from account creation), so that is upgraded in place.
    /// </summary>
    public async Task EnsureActive(Guid memberId, Guid siteSubscriptionId, Guid siteSubscriptionPriceId)
    {
        const string sql =
            """
            IF NOT EXISTS (SELECT 1 FROM MemberSiteSubscriptionLog WHERE MemberId = @memberId AND IsCurrent = 1)
                INSERT INTO MemberSiteSubscriptionLog
                    (Id, MemberId, SiteSubscriptionId, SiteSubscriptionPriceId, ExpiresUtc, CreatedUtc, IsCurrent)
                VALUES (NEWID(), @memberId, @subId, @priceId, @expires, GETUTCDATE(), 1);
            ELSE
                UPDATE MemberSiteSubscriptionLog
                SET SiteSubscriptionId = @subId, SiteSubscriptionPriceId = @priceId, ExpiresUtc = @expires
                WHERE MemberId = @memberId AND IsCurrent = 1;
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@subId", siteSubscriptionId)
            .AddParameter("@priceId", siteSubscriptionPriceId)
            .AddParameter("@expires", DateTime.UtcNow.AddYears(1));

        await builder.ExecuteNonQuery();
    }

    /// <summary>The member's current site-subscription expiry (UTC), or null if they have no current record yet.</summary>
    public async Task<DateTime?> GetExpiresUtc(Guid memberId)
    {
        const string sql =
            "SELECT ExpiresUtc FROM MemberSiteSubscriptionLog WHERE MemberId = @memberId AND IsCurrent = 1";

        await using var builder = Builder(sql).AddParameter("@memberId", memberId);
        return await builder.ExecuteScalar<DateTime?>();
    }

    /// <summary>
    /// The payment provider's subscription id on the member's current record, or null if they have no
    /// current record or it was not created by a purchase. Cancellation is driven entirely through the
    /// provider using this id, so a test polls for it rather than for the expiry.
    /// </summary>
    public async Task<string?> GetExternalId(Guid memberId)
    {
        const string sql =
            "SELECT ExternalId FROM MemberSiteSubscriptionLog WHERE MemberId = @memberId AND IsCurrent = 1";

        await using var builder = Builder(sql).AddParameter("@memberId", memberId);
        return await builder.ExecuteScalar<string?>();
    }
}
