using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;

namespace ODK.E2E.Tests;

/// <summary>
/// Recurring site-subscription renewal via a Stripe test clock (Simulations). The app's Checkout can't put
/// its customer on a clock, so this SDK-creates a customer + subscription on a fresh clock, tagged with the
/// metadata the webhook processing needs (MemberId, SiteSubscriptionPriceId, Platform), then advances the
/// clock a month to fire a real renewal webhook over the ngrok tunnel. Asserts the first invoice sets expiry
/// ~1 month out and the renewal extends it to ~2 months (a single extension per event - guarding
/// UpdateMemberSiteSubscription's extend/idempotency). Webhook-only, so the tunnel must be up. It doesn't
/// exercise the purchase UI (test clocks require an SDK-created subscription); #2 covers real Checkout.
/// </summary>
[TestFixture]
[Category("Stripe")]
public class RecurringSiteSubscriptionRenewalTests : DefaultPageTest
{
    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    private static SitePaymentSettingsDataHelper PaymentSettings => new(E2ESettings.ConnectionString);

    private static SiteSubscriptionDataHelper Subscriptions => new(E2ESettings.ConnectionString);

    [Test]
    public async Task RecurringSiteSubscription_RenewsViaWebhook_ExtendsExpiryOncePerPeriod()
    {
        // Arrange - renewals arrive as real Stripe webhooks over the tunnel; the SDK and app share the Stripe
        // account (same secret) so the app's recurring price is usable here.
        await StripeWebhookTunnel.EnsureReachable(E2ESettings.StripeWebhookBaseUrl);
        await PaymentSettings.EnsureStripeSettings(E2ESettings.StripeApiPublicKey, E2ESettings.StripeApiSecretKey);
        var subscription = await Provisioning.EnsurePurchasableSiteSubscription();
        var priceExternalId = await Subscriptions.GetPriceExternalId(subscription.Id)
            ?? throw new InvalidOperationException("The recurring site subscription has no Stripe price id.");

        var member = await Provisioning.NewAccount("renewal-member");
        var memberId = await Members.GetMemberId(member.Email);

        // Metadata the app's site-subscription webhook processing requires (PascalCase keys). Platform must
        // be "Default" - it scopes the member's subscription lookup/insert. SiteSubscriptionPriceId is the
        // app's internal price GUID (not the Stripe price id).
        var metadata = new Dictionary<string, string>
        {
            ["MemberId"] = memberId.ToString(),
            ["SiteSubscriptionPriceId"] = subscription.PriceId.ToString(),
            ["Platform"] = "Default"
        };

        await using var clock = await StripeTestClock.CreateSubscription(priceExternalId, metadata);

        // Assert - the first invoice's webhook activates the subscription ~1 month out.
        var afterFirst = await PollForExpiryBeyond(memberId, DateTime.UtcNow.AddDays(20));
        afterFirst.Should().NotBeNull("the first invoice webhook should activate the subscription");
        afterFirst!.Value.Should().BeCloseTo(DateTime.UtcNow.AddMonths(1), TimeSpan.FromDays(4));

        // Act - advance the clock past the billing period to trigger a renewal.
        await clock.AdvanceOneMonth();

        // Assert - the renewal extends expiry by ~one more month (to ~2 months, not ~3: a single extension
        // per event, guarding against an idempotency/double-extension regression).
        var afterRenewal = await PollForExpiryBeyond(memberId, afterFirst.Value.AddDays(20));
        afterRenewal.Should().NotBeNull("the renewal webhook should extend the subscription");
        afterRenewal!.Value.Should().BeCloseTo(DateTime.UtcNow.AddMonths(2), TimeSpan.FromDays(7));
    }

    private static async Task<DateTime?> PollForExpiryBeyond(Guid memberId, DateTime threshold)
    {
        var deadline = DateTime.UtcNow.AddSeconds(120);
        while (DateTime.UtcNow < deadline)
        {
            var expiry = await MemberSubscriptions.GetExpiresUtc(memberId);
            if (expiry != null && expiry > threshold)
            {
                return expiry;
            }

            await Task.Delay(2000);
        }

        return await MemberSubscriptions.GetExpiresUtc(memberId);
    }
}
