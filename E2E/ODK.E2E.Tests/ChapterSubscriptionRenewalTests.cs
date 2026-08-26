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
/// real renewal webhook. Asserts the stored expiry equals Stripe's next payment date after both the first
/// invoice and the renewal - the invariant that keeps a membership from lapsing before, or outliving, the
/// next charge - and that each billing event appends exactly one log row. Webhook-only, so the tunnel must
/// be up. No real connected account is needed - the test-clock subscription has no transfer, and a fake
/// acct_ satisfies the create guard.
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
    public async Task RecurringChapterSubscription_RenewsViaWebhook_SetsExpiryToNextPaymentDate()
    {
        // Arrange - renewals arrive as real Stripe webhooks over the tunnel.
        await StripeWebhookTunnel.EnsureReachable(E2ESettings.StripeWebhookBaseUrl);
        var paymentSettings = await PaymentSettings.GetStripeSettings(
            PlatformTypeId, E2ESettings.StripeAccountId(PlatformTypeId));
        var siteSubscription = await Provisioning.EnsurePurchasableSiteSubscription();

        var owner = await Provisioning.NewAccount("chapter-renewal-owner");
        var group = await Provisioning.CreatePublishedGroup(owner, $"e2echaprenew{Guid.NewGuid():N}");
        var ownerId = await Members.GetMemberId(owner.Email);
        await MemberSubscriptions.EnsureActive(ownerId, siteSubscription.Id, siteSubscription.PriceId);
        await ChapterPaymentAccounts.EnsureSetupComplete(group.ChapterId, ownerId, paymentSettings.Id, "acct_e2e_fake");

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

        await using var clock = await StripeTestClock.CreateSubscription(
            paymentSettings.ApiSecretKey, priceExternalId, metadata);

        // Assert - the first invoice's webhook records the expiry as the date Stripe will next charge, read
        // from the provider rather than calculated, so the two cannot disagree.
        var firstPaymentDateUtc = await clock.GetNextPaymentDateUtc();
        var afterFirst = await PollForExpiryBeyond(memberId, group.ChapterId, DateTime.UtcNow.AddDays(20));
        afterFirst.Should().NotBeNull("the first invoice webhook should activate the chapter subscription");
        afterFirst!.Value.Should().BeCloseTo(firstPaymentDateUtc, TimeSpan.FromMinutes(1));

        // A recurring subscription persists the Stripe subscription id as the record's ExternalId (used to
        // manage/cancel the subscription) - unlike a one-off purchase, which stores none.
        (await MemberChapterSubscriptions.GetCurrentExternalId(memberId, group.ChapterId))
            .Should().StartWith("sub_", "a recurring subscription should store its Stripe subscription id");

        (await MemberChapterSubscriptions.GetRecordCount(memberId, group.ChapterId))
            .Should().Be(1, "the first invoice should append exactly one log row");

        // Act - advance the clock past the billing period to trigger a renewal.
        await clock.AdvanceOneMonth();

        // Assert - the renewal moves the expiry to the new next-payment date, a period later.
        var renewedPaymentDateUtc = await clock.GetNextPaymentDateUtc();
        renewedPaymentDateUtc.Should().BeAfter(firstPaymentDateUtc, "the clock advanced past a billing period");

        var afterRenewal = await PollForExpiryBeyond(memberId, group.ChapterId, afterFirst.Value.AddDays(20));
        afterRenewal.Should().NotBeNull("the renewal webhook should extend the chapter subscription");
        afterRenewal!.Value.Should().BeCloseTo(renewedPaymentDateUtc, TimeSpan.FromMinutes(1));

        // One row per billing event. The expiry cannot reveal a double-apply - re-processing an event writes
        // the same provider date - so the row count is what guards the idempotency.
        (await MemberChapterSubscriptions.GetRecordCount(memberId, group.ChapterId))
            .Should().Be(2, "the renewal should append exactly one further log row");
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
