using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The member's site-subscription page (<c>/account/subscription</c>, Default). Before a member has a
/// currency it shows a "choose currency" form (<c>#currencyId</c>, posting to <c>/account/currency</c>);
/// once a currency is set the prompt is gone and the purchasable subscriptions are listed. Used to cover
/// currency-choice persistence.
/// </summary>
internal class SiteSubscriptionAccountPage
{
    private readonly IPage _page;

    public SiteSubscriptionAccountPage(IPage page)
    {
        _page = page;
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

    /// <summary>Whether the "choose currency" prompt is shown (i.e. the member has no currency yet).</summary>
    public async Task<bool> IsCurrencyPromptShown() => await _page.Locator("#currencyId").CountAsync() > 0;
}
