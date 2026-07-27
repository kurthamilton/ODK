using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The member-facing chapter-subscription checkout page (Default
/// <c>/groups/{slug}/subscription/{id}/checkout</c>). Navigates to the given checkout URL and pays via
/// <see cref="StripeEmbeddedCheckout"/>. The purchase transfers to the chapter's Stripe connected account,
/// so the seeded ChapterPaymentAccount must carry a real onboarded sandbox <c>acct_</c>.
/// </summary>
internal class ChapterSubscriptionCheckoutPage
{
    private readonly IPage _page;

    public ChapterSubscriptionCheckoutPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Navigates to the checkout URL and pays with the Stripe test card, returning once Stripe has accepted
    /// the card and redirected off the checkout page. Completion is webhook-driven - poll the DB.
    /// </summary>
    public async Task PayWithTestCard(string checkoutUrl)
    {
        await _page.Navigate(checkoutUrl);
        await StripeEmbeddedCheckout.PayWithTestCard(_page);
    }
}
