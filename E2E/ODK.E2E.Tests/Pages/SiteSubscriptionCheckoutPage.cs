using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The site-subscription checkout page (<c>/account/subscription/{priceId}/checkout</c>). Navigates there
/// and pays via <see cref="StripeElementsCheckout"/> (the shared Payment Element driver).
/// </summary>
internal class SiteSubscriptionCheckoutPage
{
    private readonly IPage _page;

    public SiteSubscriptionCheckoutPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Navigates to checkout for the given price and pays with the Stripe test card, returning once Stripe
    /// has accepted the card and redirected off the checkout page. Completion is webhook-driven - poll the DB.
    /// </summary>
    public async Task PayWithTestCard(Guid priceId)
    {
        await _page.Navigate($"/account/subscription/{priceId}/checkout");
        await StripeElementsCheckout.PayWithTestCard(_page);
    }
}
