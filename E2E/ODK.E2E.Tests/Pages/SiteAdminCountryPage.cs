using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The site-admin country edit page (<c>/siteadmin/countries/{id}</c>). Read-only country fields plus a
/// table of the country's locales (locale + date format); the current default is highlighted and every
/// other row has a "Set as default" action.
/// </summary>
internal class SiteAdminCountryPage
{
    private readonly IPage _page;

    public SiteAdminCountryPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Sets the first settable locale (the first row offering "Set as default") as the country's default,
    /// and returns the locale set so the caller can assert it persisted.
    /// </summary>
    public async Task<string> SetFirstAvailableLocale(Guid countryId)
    {
        await _page.Navigate($"/siteadmin/countries/{countryId}");

        var locale = await _page.EvaluateAsync<string>(
            "() => { const b = document.querySelector('[data-set-default]'); " +
            "return b ? b.closest('tr').getAttribute('data-locale') : ''; }");
        if (string.IsNullOrEmpty(locale))
        {
            throw new InvalidOperationException($"Country '{countryId}' has no settable locales.");
        }

        await _page.Locator("[data-set-default]").First.ClickAsync();

        // Success redirects back to the same page (PRG); wait for that reload to settle.
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return locale;
    }
}
