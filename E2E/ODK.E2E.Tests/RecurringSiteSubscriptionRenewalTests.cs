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
/// clock a month to fire a real renewal webhook over the ngrok tunnel. Asserts the stored expiry equals
/// Stripe's next payment date after both the first invoice and the renewal - the invariant that keeps a
/// subscription from lapsing before, or outliving, the next charge - and that each billing event appends
/// exactly one log row. Webhook-only, so the tunnel must be up. It doesn't exercise the purchase UI (test
/// clocks require an SDK-created subscription); SiteSubscriptionPurchaseTests covers real Checkout.
///
/// The second test tags the subscription with the checkout ids as well, which is what the app's own
/// checkout writes, and follows the money instead of the expiry.
/// </summary>
[TestFixture]
[Category("Stripe")]
public class RecurringSiteSubscriptionRenewalTests : DefaultPageTest
{
    private static PaymentCheckoutSessionDataHelper CheckoutSessions => new(E2ESettings.ConnectionString);

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    private static PaymentDataHelper Payments => new(E2ESettings.ConnectionString);

    private static SiteSubscriptionDataHelper Subscriptions => new(E2ESettings.ConnectionString);

    [Test]
    public async Task RecurringSiteSubscription_RenewsViaWebhook_SetsExpiryToNextPaymentDate()
    {
        // Arrange - renewals arrive as real Stripe webhooks over the tunnel; the SDK and app share the Stripe
        // account (same secret) so the app's recurring price is usable here.
        await StripeWebhookTunnel.EnsureReachable(E2ESettings.StripeWebhookBaseUrl);
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

        await using var clock = await StripeTestClock.CreateSubscription(
            E2ESettings.StripeSecretApiKey(PlatformTypeId), priceExternalId, metadata);

        // Assert - the first invoice's webhook records the expiry as the date Stripe will next charge, read
        // from the provider rather than calculated, so the two cannot disagree.
        var firstPaymentDateUtc = await clock.GetNextPaymentDateUtc();
        var afterFirst = await PollForExpiryBeyond(memberId, DateTime.UtcNow.AddDays(20));
        afterFirst.Should().NotBeNull("the first invoice webhook should activate the subscription");
        afterFirst!.Value.Should().BeCloseTo(firstPaymentDateUtc, TimeSpan.FromMinutes(1));

        // The member starts with a free placeholder record from account creation, so the purchase is the
        // second row. Counting them is what catches a double-apply below.
        var recordsAfterFirst = await MemberSubscriptions.GetRecordCount(memberId);

        // Act - advance the clock past the billing period to trigger a renewal.
        await clock.AdvanceOneMonth();

        // Assert - the renewal moves the expiry to the new next-payment date, a period later.
        var renewedPaymentDateUtc = await clock.GetNextPaymentDateUtc();
        renewedPaymentDateUtc.Should().BeAfter(firstPaymentDateUtc, "the clock advanced past a billing period");

        var afterRenewal = await PollForExpiryBeyond(memberId, afterFirst.Value.AddDays(20));
        afterRenewal.Should().NotBeNull("the renewal webhook should extend the subscription");
        afterRenewal!.Value.Should().BeCloseTo(renewedPaymentDateUtc, TimeSpan.FromMinutes(1));

        // One row per billing event. The expiry cannot reveal a double-apply - re-processing an event writes
        // the same provider date - so the row count is what guards the idempotency.
        (await MemberSubscriptions.GetRecordCount(memberId))
            .Should().Be(recordsAfterFirst + 1, "the renewal should append exactly one further log row");
    }

    [Test]
    public async Task RecurringSiteSubscription_SubscriptionCarriesCheckoutIds_RecordsTheRenewalsOwnPayment()
    {
        // Arrange - as the renewal test above, but tagged the way the app's own checkout tags a recurring
        // subscription: StartCheckout writes one metadata dictionary to the session and to the subscription,
        // so PaymentId and PaymentCheckoutSessionId - which name the purchase that created it - travel onto
        // every invoice the subscription ever issues.
        await StripeWebhookTunnel.EnsureReachable(E2ESettings.StripeWebhookBaseUrl);
        var subscription = await Provisioning.EnsurePurchasableSiteSubscription();
        var priceExternalId = await Subscriptions.GetPriceExternalId(subscription.Id)
            ?? throw new InvalidOperationException("The recurring site subscription has no Stripe price id.");

        var member = await Provisioning.NewAccount("checkout-id-member");
        var memberId = await Members.GetMemberId(member.Email);

        // The rows checkout would have left: a payment awaiting the first charge, and the session that
        // started it. Seeded rather than bought, because a subscription created through the app's checkout
        // cannot be put on a test clock.
        var paymentId = await Payments.AddUnpaidSiteSubscriptionPayment(
                memberId, subscription.PriceId, E2ESettings.EnvironmentTypeId, PlatformTypeId)
            ?? throw new InvalidOperationException("The site subscription price has no payment to check out.");
        var checkoutSessionId = await CheckoutSessions.Add(
            memberId, paymentId, $"cs_test_e2e_{Guid.NewGuid():N}");

        var metadata = new Dictionary<string, string>
        {
            ["MemberId"] = memberId.ToString(),
            ["SiteSubscriptionPriceId"] = subscription.PriceId.ToString(),
            ["Platform"] = "Default",
            ["PaymentId"] = paymentId.ToString(),
            ["PaymentCheckoutSessionId"] = checkoutSessionId.ToString()
        };

        await using var clock = await StripeTestClock.CreateSubscription(
            E2ESettings.StripeSecretApiKey(PlatformTypeId), priceExternalId, metadata);

        // Assert - the first invoice is the purchase, so it settles the payment checkout was waiting on
        // rather than writing another.
        var afterFirst = await PollForExpiryBeyond(memberId, DateTime.UtcNow.AddDays(20));
        afterFirst.Should().NotBeNull("the first invoice webhook should activate the subscription");

        (await Payments.GetPaidUtc(paymentId))
            .Should().NotBeNull("the first invoice should mark the checkout's own payment paid");
        (await CheckoutSessions.GetCompletedUtc(checkoutSessionId))
            .Should().NotBeNull("the first invoice should complete the checkout session");
        (await Payments.GetCount(memberId))
            .Should().Be(1, "the first invoice should claim the checkout payment, not add another");

        // Act - advance the clock past the billing period to trigger a renewal.
        await clock.AdvanceOneMonth();

        var afterRenewal = await PollForExpiryBeyond(memberId, afterFirst!.Value.AddDays(20));
        afterRenewal.Should().NotBeNull("the renewal webhook should extend the subscription");

        // Assert - a renewal takes money of its own, so it has a payment of its own. The checkout payment
        // the metadata names is the first purchase's and is already paid, so it cannot be this one.
        (await Payments.GetCount(memberId))
            .Should().Be(2, "a renewal takes a further payment, so it should record one");
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
