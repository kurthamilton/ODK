namespace ODK.E2E.Data;

/// <summary>
/// Direct-to-DB setup of the live Stripe payment settings the tests need. Creating a site subscription
/// (which a DrunkenKnitwits chapter account depends on) requires an active <c>SitePaymentSettings</c>
/// row, and creating the subscription through the UI calls the real Stripe API - so the settings carry
/// real Stripe keys and are "live", ready for payment-integration tests. Cleanup deactivates them.
/// </summary>
public class SitePaymentSettingsDataHelper : DataHelperBase
{
    public const string Name = "Stripe-E2E";

    public SitePaymentSettingsDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task Deactivate()
    {
        const string sql = "UPDATE SitePaymentSettings SET Active = 0 WHERE Name = @name";

        await using var builder = Builder(sql).AddParameter("@name", Name);
        await builder.ExecuteNonQuery();
    }

    /// <summary>
    /// Ensures a live, active "Stripe-E2E" payment settings row exists (inserting it if missing, and
    /// refreshing the keys + activating it if present). Returns its id.
    /// </summary>
    public async Task<Guid> EnsureStripeSettings(string apiPublicKey, string apiSecretKey)
    {
        const string sql =
            """
            IF NOT EXISTS (SELECT 1 FROM SitePaymentSettings WHERE Name = @name)
                INSERT INTO SitePaymentSettings (Id, Provider, ApiPublicKey, ApiSecretKey, Active, Name, Commission, Enabled)
                VALUES (NEWID(), 'Stripe', @public, @secret, 1, @name, 0.05, 1);
            ELSE
                UPDATE SitePaymentSettings
                SET Active = 1, Enabled = 1, Provider = 'Stripe', ApiPublicKey = @public, ApiSecretKey = @secret
                WHERE Name = @name;

            SELECT Id FROM SitePaymentSettings WHERE Name = @name;
            """;

        await using var builder = Builder(sql)
            .AddParameter("@name", Name)
            .AddParameter("@public", apiPublicKey)
            .AddParameter("@secret", apiSecretKey);

        return await builder.ExecuteScalar<Guid>();
    }

    /// <summary>Whether a DrunkenKnitwits site subscription already exists for the given payment settings.</summary>
    public async Task<bool> DrunkenKnitwitsSubscriptionExists(Guid sitePaymentSettingId)
    {
        const string sql =
            """
            SELECT COUNT(1)
            FROM SiteSubscriptions
            WHERE SitePaymentSettingId = @id AND PlatformTypeId = 2
            """;

        await using var builder = Builder(sql).AddParameter("@id", sitePaymentSettingId);
        return await builder.ExecuteScalar<int>() > 0;
    }
}
