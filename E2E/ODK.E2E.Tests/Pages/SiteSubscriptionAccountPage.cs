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

    private readonly IPage _page;

    public SiteSubscriptionAccountPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Cancels the current subscription, waiting for the redirect back to settle. The button guards itself
    /// with <c>window.confirm</c>, which Playwright dismisses unless a handler accepts it - a dismissal
    /// returns false from the inline handler and silently blocks the submit.
    /// </summary>
    public async Task CancelSubscription()
    {
        void Accept(object? sender, IDialog dialog) => _ = dialog.AcceptAsync();

        _page.Dialog += Accept;

        try
        {
            await _page.ClickAsync(CancelButtonSelector);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        finally
        {
            _page.Dialog -= Accept;
        }
    }

    /// <summary>Selects the first available currency and submits, waiting for the redirect back to settle.</summary>
    public async Task ChooseFirstCurrency()
    {
        await _page.SelectOptionAsync("#currencyId", new SelectOptionValue { Index = 1 });
        await _page.ClickAsync("button:has-text('Update')");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>Navigates to the page.</summary>
    public Task GoTo() => _page.Navigate("/account/subscription");

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
}
