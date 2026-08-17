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

    /// <summary>Upserts a setup-complete payment account for the chapter, returning its id.</summary>
    public async Task<Guid> EnsureSetupComplete(
        Guid chapterId, Guid ownerId, Guid sitePaymentSettingId, string externalId)
    {
        const string sql =
            """
            IF NOT EXISTS (SELECT 1 FROM ChapterPaymentAccounts WHERE ChapterId = @chapterId)
                INSERT INTO ChapterPaymentAccounts
                    (Id, ChapterId, OwnerId, SitePaymentSettingId, ExternalId,
                     CreatedUtc, OnboardingCompletedUtc, IdentityDocumentsProvidedUtc)
                VALUES (NEWID(), @chapterId, @ownerId, @settingId, @externalId, @now, @now, @now);
            ELSE
                UPDATE ChapterPaymentAccounts
                SET OwnerId = @ownerId, SitePaymentSettingId = @settingId, ExternalId = @externalId,
                    OnboardingCompletedUtc = @now, IdentityDocumentsProvidedUtc = @now
                WHERE ChapterId = @chapterId;

            SELECT Id FROM ChapterPaymentAccounts WHERE ChapterId = @chapterId;
            """;

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@ownerId", ownerId)
            .AddParameter("@settingId", sitePaymentSettingId)
            .AddParameter("@externalId", externalId)
            .AddParameter("@now", DateTime.UtcNow);

        return await builder.ExecuteScalar<Guid>();
    }
}
