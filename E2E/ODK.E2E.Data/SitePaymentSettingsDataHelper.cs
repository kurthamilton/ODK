using ODK.E2E.Data.Models;

namespace ODK.E2E.Data;

/// <summary>
/// Reads the <c>SitePaymentSettings</c> row a platform transacts through. The row is not created here: it
/// carries live Stripe API keys, and the connected account the payment tests pay into exists only under the
/// Stripe account those keys belong to, so the two have to be curated together in the database rather than
/// half of each seeded from test config. Config names the Stripe account
/// (<c>Stripe:Platforms:&lt;platform&gt;:AccountId</c>) and the tests find the row whose <c>ExternalId</c>
/// is it.
///
/// Settings are per platform: the app resolves them for the platform serving the request, so a row set up
/// for one platform is invisible to the other.
/// </summary>
public class SitePaymentSettingsDataHelper : DataHelperBase
{
    public SitePaymentSettingsDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>
    /// The platform's payment settings for the given Stripe account id, throwing when the database has no
    /// such row. Throws rather than seeding one, because a row invented here would carry no usable keys -
    /// see the note on this class.
    /// </summary>
    public async Task<TestSitePaymentSettings> GetStripeSettings(int platformTypeId, string stripeAccountId)
    {
        const string sql =
            """
            SELECT Id, Name, ApiSecretKey
            FROM SitePaymentSettings
            WHERE ExternalId = @accountId AND PlatformTypeId = @platform AND Enabled = 1
            """;

        await using var builder = Builder(sql)
            .AddParameter("@accountId", stripeAccountId)
            .AddParameter("@platform", platformTypeId);

        var settings = await builder.ReadMany(x => new TestSitePaymentSettings(
            x.GetGuid(0),
            x.GetString(1),
            x.GetString(2)));

        return settings.Count == 1
            ? settings.First()
            : throw new InvalidOperationException(
                $"Expected exactly one enabled SitePaymentSettings row for platform " +
                $"{PlatformTypeIds.Name(platformTypeId)} with ExternalId '{stripeAccountId}', found " +
                $"{settings.Count}. Check the row exists and that " +
                $"'Stripe:Platforms:{PlatformTypeIds.Name(platformTypeId)}:AccountId' names its Stripe account.");
    }
}
