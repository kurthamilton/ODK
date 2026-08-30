using System.Globalization;
using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The site-admin create-subscription page (<c>/siteadmin/subscriptions/new</c>) and the price form on
/// the created subscription's detail page. Creating a subscription also creates the Stripe product, and
/// adding a paid price creates the Stripe plan (on the Stripe account the app is configured to transact
/// on), so this needs real Stripe keys. Site admin is a global role and the site-admin area is
/// platform-agnostic, but the new subscription's platform comes from the request - so run this against
/// the platform whose default subscription you're setting up (the caller drives it on the matching
/// platform's browser context).
/// </summary>
internal class SiteAdminSubscriptionsPage
{
    private readonly IPage _page;

    public SiteAdminSubscriptionsPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Adds a price to the subscription currently shown on the detail page (call after
    /// <see cref="CreateSubscription"/>, which lands there). A paid price triggers a Stripe plan creation
    /// during the POST, so the row - once it appears - carries the external plan id.
    /// </summary>
    public async Task AddPrice(string currencyCode, string frequency, decimal amount)
    {
        // Currency is picked from a dialog rather than a select, and its rows are labelled with country and
        // currency names - so the row is matched on the data-currency-code the app renders for this purpose.
        await _page.ClickAsync("[data-currency-picker-trigger]");
        await _page.ClickAsync($"[data-currency-picker-option][data-currency-code='{currencyCode}']");

        await _page.SelectOptionAsync("#Frequency", new SelectOptionValue { Label = frequency });
        await _page.FillAsync("#Amount", amount.ToString(CultureInfo.InvariantCulture));

        // The add-price form posts to /siteadmin/Subscriptions/{id}/Prices; the delete forms end in
        // /Delete, so this matches only the add form.
        await _page.ClickAsync("form[action$='/Prices'] button.btn-primary");

        // PRG back to the detail page with the new price row. If the submit was blocked (a required field)
        // or the server errored (e.g. Stripe), the row never appears - surface why rather than time out bare.
        try
        {
            await _page.Locator($"tbody tr:has-text(\"{frequency}\")").First.WaitForAsync(new() { Timeout = 20000 });
        }
        catch (TimeoutException)
        {
            var errors = await _page.Locator(
                ".field-validation-error, .text-danger, .validation-summary-errors, .alert").AllInnerTextsAsync();
            var body = await _page.InnerTextAsync("body");
            throw new InvalidOperationException(
                $"Add price did not appear. URL='{_page.Url}'. " +
                $"Validation/alerts=[{string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x)))}]. " +
                $"Body: {body[..Math.Min(500, body.Length)]}");
        }
    }

    /// <summary>
    /// Creates a site subscription. <paramref name="featureIds"/> are the numeric
    /// <c>SiteFeatureType</c> values to select (e.g. 5 = MemberSubscriptions / "Paid subscriptions");
    /// null/empty selects none. Pass <paramref name="free"/> for a subscription that needs no price - a
    /// subscription that is neither free nor priced is not usable, so nobody can be put on it.
    /// Lands on the created subscription's detail page on success.
    /// </summary>
    public async Task CreateSubscription(
        string name, string description, int groupLimit, int memberLimit,
        int[]? featureIds = null, bool free = false)
    {
        await _page.Navigate("/siteadmin/subscriptions/new");

        await _page.FillAsync("#Name", name);

        await _page.SetHtmlEditor("DescriptionHtml", description);

        if (!await _page.IsCheckedAsync("#Enabled"))
        {
            await _page.CheckAsync("#Enabled");
        }

        if (free)
        {
            await _page.CheckAsync("#Free");
        }

        await _page.FillAsync("#GroupLimit", groupLimit.ToString());
        await _page.FillAsync("#MemberLimit", memberLimit.ToString());

        // Features is a native multi-select; select the requested enum values by their option value.
        if (featureIds is { Length: > 0 })
        {
            await _page.SelectOptionAsync(
                "#Features",
                featureIds.Select(id => new SelectOptionValue { Value = id.ToString() }).ToArray());
        }

        await _page.ClickAsync("button:has-text('Create')");

        // On success the app redirects to the created subscription's page (/siteadmin/subscriptions/{id}).
        // If the submit was blocked (a required field client-side) it stays on /new; surface why.
        try
        {
            await _page.WaitForURLAsync(
                url => url.Contains("/siteadmin/subscriptions/") && !url.Contains("/new"),
                new() { Timeout = 20000 });
        }
        catch (TimeoutException)
        {
            var errors = await _page.Locator(
                ".field-validation-error, .text-danger, .validation-summary-errors, .alert").AllInnerTextsAsync();
            var body = await _page.InnerTextAsync("body");
            throw new InvalidOperationException(
                $"Create subscription did not navigate. URL='{_page.Url}'. " +
                $"Validation/alerts=[{string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x)))}]. " +
                $"Body: {body[..Math.Min(500, body.Length)]}");
        }
    }
}
