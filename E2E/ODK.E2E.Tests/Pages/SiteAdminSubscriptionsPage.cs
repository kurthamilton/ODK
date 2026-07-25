using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The site-admin create-subscription page (<c>/siteadmin/subscriptions/new</c>). Creating a
/// subscription also creates the Stripe product (via the selected live payment settings), so this needs
/// real Stripe keys. Site admin is a global role and the site-admin area is platform-agnostic, but the
/// new subscription's platform comes from the request - so run this against the platform whose default
/// subscription you're setting up (the caller drives it on a DrunkenKnitwits-context browser).
/// </summary>
internal class SiteAdminSubscriptionsPage
{
    private readonly IPage _page;

    public SiteAdminSubscriptionsPage(IPage page)
    {
        _page = page;
    }

    public async Task CreateSubscription(
        string paymentSettingName, string name, string description, int groupLimit, int memberLimit)
    {
        await _page.Navigate("/siteadmin/subscriptions/new");

        await _page.SelectOptionAsync("#SitePaymentSettingId", new SelectOptionValue { Label = paymentSettingName });
        await _page.FillAsync("#Name", name);

        // Description is a TinyMCE editor over a hidden textarea, so it can't be filled directly. Use the
        // TinyMCE API to set the content and save() it back to the textarea (so client validation sees a
        // value and the form submits it). Wait for the editor to finish initialising first.
        await _page.WaitForFunctionAsync(
            "() => { const ed = window.tinymce && window.tinymce.get('Description'); return !!ed && ed.initialized === true; }");
        await _page.EvaluateAsync(
            "value => { const ed = window.tinymce.get('Description'); ed.setContent(value); ed.save(); }",
            description);

        if (!await _page.IsCheckedAsync("#Enabled"))
        {
            await _page.CheckAsync("#Enabled");
        }

        await _page.FillAsync("#GroupLimit", groupLimit.ToString());
        await _page.FillAsync("#MemberLimit", memberLimit.ToString());

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
