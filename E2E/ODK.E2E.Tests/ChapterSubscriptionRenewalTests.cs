using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;

namespace ODK.E2E.Tests;

/// <summary>
/// Recurring chapter-subscription renewal via a Stripe test clock (Simulations) - the chapter (member ->
/// chapter) counterpart of the site-subscription renewal test. The owner creates a recurring chapter
/// subscription (its Stripe recurring price lives on the platform account); this SDK-creates a customer +
/// subscription on that price on a fresh clock, tagged with the chapter metadata the webhook processing
/// needs (MemberId, ChapterId, ChapterSubscriptionId, Platform), then advances the clock a month to fire a
/// real renewal webhook. Asserts the first invoice sets the member's chapter subscription ~1 month out and
/// the renewal extends it to ~2 months (a single extension per event, guarding UpdateMemberChapterSubscription).
/// Webhook-only, so the tunnel must be up. No real connected account is needed - the test-clock subscription
/// has no transfer, and a fake acct_ satisfies the create guard.
/// </summary>
[TestFixture]
public class ChapterSubscriptionRenewalTests : DefaultPageTest
{
    private static ChapterPaymentAccountDataHelper ChapterPaymentAccounts => new(E2ESettings.ConnectionString);

    private static ChapterSubscriptionDataHelper ChapterSubscriptions => new(E2ESettings.ConnectionString);

    private static MemberChapterSubscriptionDataHelper MemberChapterSubscriptions => new(E2ESettings.ConnectionString);

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    private static SitePaymentSettingsDataHelper PaymentSettings => new(E2ESettings.ConnectionString);

    [Test]
    public async Task RecurringChapterSubscription_RenewsViaWebhook_ExtendsExpiryOncePerPeriod()
    {
        // Arrange - renewals arrive as real Stripe webhooks over the tunnel.
        await StripeWebhookTunnel.EnsureReachable(E2ESettings.StripeWebhookBaseUrl);
        var settingsId = await PaymentSettings.EnsureStripeSettings(
            E2ESettings.StripeApiPublicKey, E2ESettings.StripeApiSecretKey);
        var siteSubscription = await Provisioning.EnsurePurchasableSiteSubscription();

        var owner = await Provisioning.NewAccount("chapter-renewal-owner");
        var group = await Provisioning.CreatePublishedGroup(owner, $"e2echaprenew{Guid.NewGuid():N}");
        var ownerId = await Members.GetMemberId(owner.Email);
        await MemberSubscriptions.EnsureActive(ownerId, siteSubscription.Id, siteSubscription.PriceId);
        await ChapterPaymentAccounts.EnsureSetupComplete(group.ChapterId, ownerId, settingsId, "acct_e2e_fake");

        // The owner creates a recurring chapter subscription; its Stripe recurring price (on the platform
        // account) is what the SDK subscribes the test-clock customer to.
        var subscriptionName = $"e2e-chaptersub-{Guid.NewGuid():N}";
        await Provisioning.CreateChapterSubscription(
            group, owner, subscriptionName, amount: 5m, durationMonths: 1, recurring: true);
        var subscriptionId = await ChapterSubscriptions.GetId(group.ChapterId, subscriptionName)
            ?? throw new InvalidOperationException($"Chapter subscription '{subscriptionName}' was not created.");
        var priceExternalId = await ChapterSubscriptions.GetExternalId(group.ChapterId, subscriptionName)
            ?? throw new InvalidOperationException($"Chapter subscription '{subscriptionName}' has no Stripe price id.");

        // A fresh member joins the chapter (membership is required for the renewal to record against them).
        var member = await Provisioning.JoinGroupAsMember(group);
        var memberId = await Members.GetMemberId(member.Email);

        // Metadata the app's chapter-subscription webhook processing requires (PascalCase keys).
        var metadata = new Dictionary<string, string>
        {
            ["MemberId"] = memberId.ToString(),
            ["ChapterId"] = group.ChapterId.ToString(),
            ["ChapterSubscriptionId"] = subscriptionId.ToString(),
            ["Platform"] = "Default"
        };

        await using var clock = await StripeTestClock.CreateSubscription(priceExternalId, metadata);

        // Assert - the first invoice's webhook activates the member's chapter subscription ~1 month out.
        var afterFirst = await PollForExpiryBeyond(memberId, group.ChapterId, DateTime.UtcNow.AddDays(20));
        afterFirst.Should().NotBeNull("the first invoice webhook should activate the chapter subscription");
        afterFirst!.Value.Should().BeCloseTo(DateTime.UtcNow.AddMonths(1), TimeSpan.FromDays(4));

        // A recurring subscription persists the Stripe subscription id as the record's ExternalId (used to
        // manage/cancel the subscription) - unlike a one-off purchase, which stores none.
        (await MemberChapterSubscriptions.GetCurrentExternalId(memberId, group.ChapterId))
            .Should().StartWith("sub_", "a recurring subscription should store its Stripe subscription id");

        // Act - advance the clock past the billing period to trigger a renewal.
        await clock.AdvanceOneMonth();

        // Assert - the renewal extends expiry by ~one more month (to ~2 months, not ~3: a single extension
        // per event, guarding against an idempotency/double-extension regression).
        var afterRenewal = await PollForExpiryBeyond(memberId, group.ChapterId, afterFirst.Value.AddDays(20));
        afterRenewal.Should().NotBeNull("the renewal webhook should extend the chapter subscription");
        afterRenewal!.Value.Should().BeCloseTo(DateTime.UtcNow.AddMonths(2), TimeSpan.FromDays(7));
    }

    private static async Task<DateTime?> PollForExpiryBeyond(Guid memberId, Guid chapterId, DateTime threshold)
    {
        var deadline = DateTime.UtcNow.AddSeconds(120);
        while (DateTime.UtcNow < deadline)
        {
            var expiry = await MemberChapterSubscriptions.GetExpiryUtc(memberId, chapterId);
            if (expiry != null && expiry > threshold)
            {
                return expiry;
            }

            await Task.Delay(2000);
        }

        return await MemberChapterSubscriptions.GetExpiryUtc(memberId, chapterId);
    }
}
