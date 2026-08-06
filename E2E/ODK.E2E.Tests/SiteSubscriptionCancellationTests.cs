using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Cancellation of a member's own site subscription from <c>/account/subscription</c> (Default). The cancel
/// form only renders while the payment provider reports the subscription active, so there is no shortcut
/// around a real purchase: the test buys one through the embedded Checkout with a test card and waits for the
/// webhook to record the Stripe subscription id that cancellation is driven by. Requires live Stripe keys AND
/// a running ngrok tunnel (<c>Stripe:WebhookBaseUrl</c>), preflighted via <see cref="StripeWebhookTunnel"/>.
///
/// Cancelling writes nothing locally - the app calls Stripe and the page re-reads the status on each render -
/// so the assertions are the page's own state rather than the database.
/// </summary>
[TestFixture]
public class SiteSubscriptionCancellationTests : DefaultPageTest
{
    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    [Test]
    public async Task CancelSiteSubscription_WithActiveSubscription_CancelsAndWithdrawsTheCancelForm()
    {
        // Arrange - a fresh buyer with a real, active subscription. The subscription is shared (created once,
        // never mutated); the buyer is local because the test cancels their subscription.
        await StripeWebhookTunnel.EnsureReachable(E2ESettings.StripeWebhookBaseUrl);
        var subscription = await Provisioning.EnsurePurchasableSiteSubscription();
        var buyer = await Provisioning.NewAccount("site-subscription-canceller");
        var buyerId = await Members.GetMemberId(buyer.Email);
        await new LoginPage(Page).LogIn(buyer.Email, buyer.Password);

        // The page shows only a currency prompt until the member has a currency, and the current subscription
        // (with its cancel form) renders past that point.
        var accountPage = new SiteSubscriptionAccountPage(Page);
        await accountPage.GoTo();
        if (await accountPage.IsCurrencyPromptShown())
        {
            await accountPage.ChooseFirstCurrency();
        }

        await new SiteSubscriptionCheckoutPage(Page).PayWithTestCard(subscription.PriceId);

        // Completion is webhook-only, so poll - and poll for the external id specifically, since that is what
        // cancellation needs (a record without one fails with "External subscription not found").
        var externalId = await PollForExternalSubscriptionId(buyerId);
        externalId.Should().NotBeNullOrEmpty(
            "the payment webhook should record the provider's subscription id against the member");

        await accountPage.GoTo();
        (await accountPage.IsCancelButtonShown()).Should().BeTrue(
            "an active subscription should offer cancellation");

        // Act
        await accountPage.CancelSubscription();

        // Assert - the POST reached the handler and the provider call succeeded. The badge and the withdrawn
        // form both come from the provider's status re-read on this render, so they only appear once Stripe
        // has actually cancelled it.
        (await accountPage.HasFeedback("Subscription cancelled")).Should().BeTrue(
            "cancelling should report success");
        (await accountPage.IsCancelledBadgeShown()).Should().BeTrue(
            "the provider should now report the subscription cancelled");
        (await accountPage.IsCancelButtonShown()).Should().BeFalse(
            "a cancelled subscription should not offer cancellation again");
    }

    private static async Task<string?> PollForExternalSubscriptionId(Guid memberId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(90);
        while (DateTime.UtcNow < deadline)
        {
            var externalId = await MemberSubscriptions.GetExternalId(memberId);
            if (!string.IsNullOrEmpty(externalId))
            {
                return externalId;
            }

            await Task.Delay(2000);
        }

        return await MemberSubscriptions.GetExternalId(memberId);
    }
}
