using System.Globalization;
using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The chapter-admin create-subscription page (Default <c>.../members/subscriptions/new</c>, DrunkenKnitwits
/// <c>.../create</c>). The form posts back to itself and, on success, redirects to the subscriptions list.
/// Requires the chapter owner to have the MemberSubscriptions site feature and a set-up payment account
/// (the test seeds both). Description is a TinyMCE editor; recurring is a checkbox - leave it unchecked for a
/// non-recurring / one-off subscription. Currency is fixed to the chapter's currency (no selector).
/// </summary>
internal class ChapterSubscriptionAdminPage
{
    private readonly IPage _page;

    public ChapterSubscriptionAdminPage(IPage page)
    {
        _page = page;
    }

    public async Task CreateSubscription(
        string createUrl, string name, string title, string description, decimal amount, int durationMonths,
        bool recurring)
    {
        await _page.Navigate(createUrl);

        await _page.FillAsync("#Name", name);
        await _page.FillAsync("#Title", title);
        await _page.FillAsync("#Amount", amount.ToString(CultureInfo.InvariantCulture));
        await _page.FillAsync("#DurationMonths", durationMonths.ToString());

        await _page.SetHtmlEditor("Description", description);

        // Recurring is a checkbox only when the payment provider supports recurring payments; otherwise it's
        // a hidden input already fixed to false. Set it only when it's an actual checkbox.
        var recurringInput = _page.Locator("#Recurring");
        if (await recurringInput.GetAttributeAsync("type") == "checkbox")
        {
            await recurringInput.SetCheckedAsync(recurring);
        }

        if (!await _page.IsCheckedAsync("#Enabled"))
        {
            await _page.CheckAsync("#Enabled");
        }

        await _page.ClickAsync("button:has-text('Create')");

        // On success the app redirects to the subscriptions list. If a required field failed client-side or
        // the server re-rendered with an error, it stays on the create page - surface why.
        try
        {
            await _page.WaitForURLAsync(
                url => url.Contains("/members/subscriptions") && !url.Contains("/new") && !url.Contains("/create"),
                new() { Timeout = 20000 });
        }
        catch (TimeoutException)
        {
            var errors = await _page.Locator(
                ".field-validation-error, .text-danger, .validation-summary-errors, .alert").AllInnerTextsAsync();
            var body = await _page.InnerTextAsync("body");
            throw new InvalidOperationException(
                $"Create chapter subscription did not navigate. URL='{_page.Url}'. " +
                $"Validation/alerts=[{string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x)))}]. " +
                $"Body: {body[..Math.Min(500, body.Length)]}");
        }
    }
}
