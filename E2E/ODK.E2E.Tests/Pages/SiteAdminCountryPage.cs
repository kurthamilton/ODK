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
    /// The first settable locale on the country's page (the first row offering "Set as default"), or null if
    /// the country has none - not every country maps to more than one .NET culture, so callers scan for one
    /// that does rather than assuming a given country offers a choice.
    /// </summary>
    public async Task<string?> GetFirstSettableLocale(Guid countryId)
    {
        await _page.Navigate($"/siteadmin/countries/{countryId}");

        var locale = await _page.EvaluateAsync<string?>(
            "() => { const b = document.querySelector('[data-set-default]'); " +
            "return b ? b.closest('tr').getAttribute('data-locale') : null; }");
        return string.IsNullOrEmpty(locale) ? null : locale;
    }

    /// <summary>Sets the given locale as the country's default by clicking that row's "Set as default".</summary>
    public async Task SetLocaleAsDefault(Guid countryId, string locale)
    {
        await _page.Navigate($"/siteadmin/countries/{countryId}");

        await _page.Locator($"tr[data-locale='{locale}'] [data-set-default]").ClickAsync();

        // Success redirects back to the same page (PRG); wait for that reload to settle.
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
