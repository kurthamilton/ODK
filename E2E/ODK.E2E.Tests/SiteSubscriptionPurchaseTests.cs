using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Stripe purchase scenario: a Default member buys a site subscription. Completion is webhook-only, so the
/// test drives the embedded Stripe Checkout with a test card, then polls the DB until the buyer's
/// MemberSiteSubscription is active. Requires live Stripe keys AND a running ngrok tunnel
/// (<c>Stripe:WebhookBaseUrl</c>) delivering webhooks to the local app - preflighted via
/// <see cref="StripeWebhookTunnel"/>, so a missing tunnel fails fast rather than hanging.
/// </summary>
[TestFixture]
public class SiteSubscriptionPurchaseTests : DefaultPageTest
{
    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    [Test]
    public async Task PurchaseSiteSubscription_CompletesViaWebhook_ActivatesSubscription()
    {
        // Arrange - completion is webhook-only, so the tunnel must be up; a purchasable subscription (shared,
        // created once) and a fresh buyer.
        await StripeWebhookTunnel.EnsureReachable(E2ESettings.StripeWebhookBaseUrl);
        var subscription = await Provisioning.EnsurePurchasableSiteSubscription();
        var buyer = await Provisioning.NewAccount("site-subscription-buyer");
        var buyerId = await Members.GetMemberId(buyer.Email);
        await new LoginPage(Page).LogIn(buyer.Email, buyer.Password);

        // Act - pay with the Stripe test card; the invoice.payment_succeeded webhook completes the purchase.
        await new SiteSubscriptionCheckoutPage(Page).PayWithTestCard(subscription.PriceId);

        // Assert - the buyer's site subscription becomes active (webhook-driven, so poll for it).
        var expiresUtc = await PollForActiveSubscription(buyerId);
        expiresUtc.Should().NotBeNull("the site subscription should be activated by the payment webhook");

        // A single monthly purchase should set expiry ~1 month out. Asserting it's close to one month (not
        // ~two) guards against a completion-idempotency regression double-extending the subscription.
        expiresUtc!.Value.Should().BeCloseTo(DateTime.UtcNow.AddMonths(1), TimeSpan.FromDays(3));
    }

    private static async Task<DateTime?> PollForActiveSubscription(Guid memberId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(90);
        while (DateTime.UtcNow < deadline)
        {
            var expiresUtc = await MemberSubscriptions.GetExpiresUtc(memberId);
            if (expiresUtc != null && expiresUtc > DateTime.UtcNow)
            {
                return expiresUtc;
            }

            await Task.Delay(2000);
        }

        return await MemberSubscriptions.GetExpiresUtc(memberId);
    }
}
