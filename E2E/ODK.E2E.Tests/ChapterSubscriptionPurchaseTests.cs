using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Chapter-member Stripe purchase: a member buys a non-recurring chapter subscription on the Default
/// platform. The chapter's Connect payment account is seeded with a REAL onboarded sandbox connected
/// account (<c>Stripe:ConnectedAccountId</c>), because the purchase transfers funds to it and Stripe rejects
/// an un-onboarded destination. The owner creates the subscription (real Stripe product/price) on a
/// throwaway browser; a fresh member joins the chapter (membership is required for completion) and pays via
/// embedded Checkout. Completion is webhook-only, so the ngrok tunnel must be up; the test polls until the
/// member's chapter subscription is recorded and active. This exercises the one-off
/// (<c>checkout.session.completed</c>) completion path, distinct from the recurring site-subscription test.
/// </summary>
[TestFixture]
[Category("Stripe")]
public class ChapterSubscriptionPurchaseTests : DefaultPageTest
{
    private static ChapterPaymentAccountDataHelper ChapterPaymentAccounts => new(E2ESettings.ConnectionString);

    private static ChapterSubscriptionDataHelper ChapterSubscriptions => new(E2ESettings.ConnectionString);

    private static MemberChapterSubscriptionDataHelper MemberChapterSubscriptions => new(E2ESettings.ConnectionString);

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    private static SitePaymentSettingsDataHelper PaymentSettings => new(E2ESettings.ConnectionString);

    [Test]
    public async Task PurchaseChapterSubscription_CompletesViaWebhook_RecordsMemberSubscription()
    {
        // Arrange - a purchase needs the webhook tunnel up and a real onboarded connected account.
        await StripeWebhookTunnel.EnsureReachable(E2ESettings.StripeWebhookBaseUrl);
        if (string.IsNullOrWhiteSpace(E2ESettings.StripeConnectedAccountId))
        {
            Assert.Fail(
                "Set 'Stripe:ConnectedAccountId' to a pre-onboarded Stripe sandbox connected account (acct_...). " +
                "A chapter-subscription purchase transfers funds to it, and Stripe rejects an un-onboarded destination.");
        }

        var settingsId = await PaymentSettings.EnsureStripeSettings(
            E2ESettings.StripeApiPublicKey, E2ESettings.StripeApiSecretKey);
        var siteSubscription = await Provisioning.EnsurePurchasableSiteSubscription();

        var owner = await Provisioning.NewAccount("chapter-subscription-owner");
        var group = await Provisioning.CreatePublishedGroup(owner, $"e2echapbuy{Guid.NewGuid():N}");
        var ownerId = await Members.GetMemberId(owner.Email);
        await MemberSubscriptions.EnsureActive(ownerId, siteSubscription.Id, siteSubscription.PriceId);
        await ChapterPaymentAccounts.EnsureSetupComplete(
            group.ChapterId, ownerId, settingsId, E2ESettings.StripeConnectedAccountId);

        // The owner creates a non-recurring chapter subscription (real Stripe product/price), on a throwaway
        // browser so this test's own browser is free for the buyer.
        var subscriptionName = $"e2e-chaptersub-{Guid.NewGuid():N}";
        await Provisioning.CreateChapterSubscription(
            group, owner, subscriptionName, amount: 5m, durationMonths: 1, recurring: false);
        var subscriptionId = await ChapterSubscriptions.GetId(group.ChapterId, subscriptionName)
            ?? throw new InvalidOperationException($"Chapter subscription '{subscriptionName}' was not created.");

        // A fresh member joins the chapter (membership is required for the purchase to complete).
        var member = await Provisioning.JoinGroupAsMember(group);
        var memberId = await Members.GetMemberId(member.Email);
        await new LoginPage(Page).LogIn(member.Email, member.Password);

        // Act - pay via embedded Checkout; the checkout.session.completed webhook records the subscription.
        await new ChapterSubscriptionCheckoutPage(Page).PayWithTestCard(
            PlatformRoutes.Default(group).SubscriptionCheckout(subscriptionId));

        // Assert - the purchase is recorded for this member + subscription, and their chapter subscription is
        // active (webhook-driven, so poll).
        (await PollForPurchaseRecord(memberId, subscriptionId))
            .Should().BeTrue("the purchase webhook should record the member's chapter subscription");
        var expiryUtc = await MemberChapterSubscriptions.GetExpiryUtc(memberId, group.ChapterId);
        expiryUtc.Should().NotBeNull();

        // The subscription's DurationMonths is 1, so a single purchase should set expiry ~1 month out.
        // Asserting it's close to one month (not ~two) guards against a completion-idempotency regression
        // double-extending the subscription.
        expiryUtc!.Value.Should().BeCloseTo(DateTime.UtcNow.AddMonths(1), TimeSpan.FromDays(3));

        // A one-off (non-recurring) purchase must NOT persist an ExternalId - the payment-intent id is not a
        // subscription, so storing it only produces "no such subscription" noise on later Stripe lookups.
        (await MemberChapterSubscriptions.GetCurrentExternalId(memberId, group.ChapterId))
            .Should().BeNull("a non-recurring purchase should not store an external subscription id");
    }

    private static async Task<bool> PollForPurchaseRecord(Guid memberId, Guid chapterSubscriptionId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(90);
        while (DateTime.UtcNow < deadline)
        {
            if (await MemberChapterSubscriptions.HasSubscriptionRecord(memberId, chapterSubscriptionId))
            {
                return true;
            }

            await Task.Delay(2000);
        }

        return await MemberChapterSubscriptions.HasSubscriptionRecord(memberId, chapterSubscriptionId);
    }
}
