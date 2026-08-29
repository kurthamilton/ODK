namespace ODK.E2E.Data;

/// <summary>
/// Seeds a chapter's Stripe Connect payment account (<c>ChapterPaymentAccounts</c>) as fully onboarded, so
/// chapter-subscription creation's "payment account set up" guard passes without driving Stripe's hosted
/// onboarding UI. Onboarding completeness is two nullable timestamps (<c>OnboardingCompletedUtc</c> +
/// <c>IdentityDocumentsProvidedUtc</c>); setting both makes <c>ChapterPaymentAccount.SetupComplete()</c>
/// true, and the app never re-checks Stripe once they're set. For a real member purchase the
/// <c>ExternalId</c> must be a genuine onboarded sandbox account (<c>acct_...</c>); a fake id is fine for
/// create-only tests (nothing contacts Stripe about the connected account until a member checks out).
/// </summary>
public class ChapterPaymentAccountDataHelper : DataHelperBase
{
    public ChapterPaymentAccountDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>
    /// Upserts a setup-complete payment account for the chapter, returning its id. The account is stamped
    /// with the deployment and provider it belongs to, which is what the app matches it on - a row left
    /// unstamped defaults to <c>None</c> and is invisible to every environment, including this one. A
    /// chapter has one account per deployment and provider, so that is what the upsert matches on too.
    /// </summary>
    public async Task<Guid> EnsureSetupComplete(
        Guid chapterId,
        Guid ownerId,
        string externalId,
        int environmentTypeId,
        int paymentProviderTypeId = PaymentProviderTypeIds.Stripe)
    {
        const string sql =
            """
            IF NOT EXISTS (
                SELECT 1 FROM ChapterPaymentAccounts
                WHERE ChapterId = @chapterId
                    AND EnvironmentTypeId = @environmentTypeId
                    AND PaymentProviderTypeId = @paymentProviderTypeId)
                INSERT INTO ChapterPaymentAccounts
                    (Id, ChapterId, OwnerId, ExternalId, EnvironmentTypeId, PaymentProviderTypeId,
                     CreatedUtc, OnboardingCompletedUtc, IdentityDocumentsProvidedUtc)
                VALUES (NEWID(), @chapterId, @ownerId, @externalId, @environmentTypeId,
                        @paymentProviderTypeId, @now, @now, @now);
            ELSE
                UPDATE ChapterPaymentAccounts
                SET OwnerId = @ownerId, ExternalId = @externalId,
                    OnboardingCompletedUtc = @now, IdentityDocumentsProvidedUtc = @now
                WHERE ChapterId = @chapterId
                    AND EnvironmentTypeId = @environmentTypeId
                    AND PaymentProviderTypeId = @paymentProviderTypeId;

            SELECT Id FROM ChapterPaymentAccounts
            WHERE ChapterId = @chapterId
                AND EnvironmentTypeId = @environmentTypeId
                AND PaymentProviderTypeId = @paymentProviderTypeId;
            """;

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@ownerId", ownerId)
            .AddParameter("@externalId", externalId)
            .AddParameter("@environmentTypeId", environmentTypeId)
            .AddParameter("@paymentProviderTypeId", paymentProviderTypeId)
            .AddParameter("@now", DateTime.UtcNow);

        return await builder.ExecuteScalar<Guid>();
    }
}
