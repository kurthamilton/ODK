using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The member's site-subscription page (<c>/account/subscription</c>, Default). Before a member has a
/// currency it shows a "choose currency" form (<c>#currencyId</c>, posting to <c>/account/currency</c>);
/// once a currency is set the prompt is gone and the purchasable subscriptions are listed, along with the
/// current subscription and - while the payment provider reports it active - a cancel form. Used to cover
/// currency-choice persistence and cancellation.
/// </summary>
internal class SiteSubscriptionAccountPage
{
    private const string CancelButtonSelector = "button:has-text('Cancel subscription')";

    private const string Path = "/account/subscription";

    private readonly IPage _page;

    public SiteSubscriptionAccountPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Cancels the current subscription, waiting for the redirect back to settle. The form is guarded by the
    /// shared confirmation dialog, so the click alone doesn't submit - it has to be accepted.
    /// </summary>
    public async Task CancelSubscription()
    {
        await _page.ClickAsync(CancelButtonSelector);
        await _page.AcceptConfirm();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>Selects the first available currency and submits, waiting for the redirect back to settle.</summary>
    public async Task ChooseFirstCurrency()
    {
        await _page.SelectOptionAsync("#currencyId", new SelectOptionValue { Index = 1 });
        await _page.ClickAsync("button:has-text('Update')");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>Navigates to the page.</summary>
    public Task GoTo() => _page.Navigate(Path);

    /// <summary>
    /// Whether a feedback toast carrying the given message is present. Matched in the DOM rather than on
    /// screen: the toast container renders hidden and is moved into place by script.
    /// </summary>
    public async Task<bool> HasFeedback(string message)
        => await _page.Locator($".toasts:has-text('{message}')").CountAsync() > 0;

    /// <summary>Whether the cancel form is offered (only while the provider reports the subscription active).</summary>
    public async Task<bool> IsCancelButtonShown() => await _page.Locator(CancelButtonSelector).CountAsync() > 0;

    /// <summary>Whether the current subscription is badged as cancelled.</summary>
    public async Task<bool> IsCancelledBadgeShown()
        => await _page.Locator(".badge:has-text('Cancelled')").CountAsync() > 0;

    /// <summary>Whether the "choose currency" prompt is shown (i.e. the member has no currency yet).</summary>
    public async Task<bool> IsCurrencyPromptShown() => await _page.Locator("#currencyId").CountAsync() > 0;

    /// <summary>
    /// Waits for the app to land here itself after a checkout. A completed checkout returns to the confirm
    /// page, which polls the payment status every second and, as soon as the webhook has recorded it,
    /// reloads itself and redirects here - so a test must not navigate while that page is open: its own
    /// navigation is aborted by the page's (net::ERR_ABORTED), and the webhook a purchase test waits for is
    /// exactly what sets the reload off. Matched on the path alone, so the confirm page's own address (which
    /// starts with it) does not satisfy it.
    /// </summary>
    public Task WaitForCheckoutHandover()
        => _page.WaitForURLAsync(url => new Uri(url).AbsolutePath == Path, new() { Timeout = 30000 });
}
