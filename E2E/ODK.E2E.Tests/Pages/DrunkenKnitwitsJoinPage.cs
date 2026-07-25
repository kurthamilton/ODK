using System.Text.RegularExpressions;
using Microsoft.Playwright;
using ODK.E2E.Tests.Helpers;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The DrunkenKnitwits chapter join page (<c>/{chapterName}/account/join</c>). On DrunkenKnitwits,
/// joining a chapter IS the sign-up: it creates the account and sends an activation email, landing on
/// the chapter "check your email" page. The chapter's profile form is configurable, so optional fields
/// (location, privacy policy, picture) are filled only when present.
/// </summary>
internal class DrunkenKnitwitsJoinPage
{
    private readonly IPage _page;

    public DrunkenKnitwitsJoinPage(IPage page)
    {
        _page = page;
    }

    public async Task Join(string chapterShortName, string firstName, string lastName, string email)
    {
        await _page.Navigate($"/{chapterShortName}/account/join");

        await _page.FillAsync("[data-firstname]", firstName);
        await _page.FillAsync("[data-lastname]", lastName);
        await _page.FillAsync("[data-email]", email);

        await FillLocationIfPresent();
        await CheckPrivacyPolicyIfPresent();
        await UploadImageIfPresent();

        await _page.ClickAsync("button:has-text('Create')");

        // A new DrunkenKnitwits member lands on the chapter "check your email" page,
        // /{chapterName}/Account/Pending (chapter-scoped and PascalCased) - so match case-insensitively
        // on the path suffix, not the global lowercase /account/pending. If the submit was blocked (a
        // required field client-side, or the server re-rendered with errors) it stays on the join page
        // and never navigates - surface why instead of a bare 30s timeout.
        try
        {
            await _page.WaitForURLAsync(
                new Regex("/account/pending", RegexOptions.IgnoreCase), new() { Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            var errors = await _page.Locator(
                ".field-validation-error, .text-danger, .validation-summary-errors, .alert").AllInnerTextsAsync();
            var body = await _page.InnerTextAsync("body");
            throw new InvalidOperationException(
                $"DrunkenKnitwits join did not reach /account/pending. URL='{_page.Url}'. " +
                $"Validation/alerts=[{string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x)))}]. " +
                $"Body: {body[..Math.Min(500, body.Length)]}");
        }
    }

    private async Task CheckPrivacyPolicyIfPresent()
    {
        if (await _page.Locator("#PrivacyPolicy").CountAsync() > 0)
        {
            await _page.CheckAsync("#PrivacyPolicy");
        }
    }

    private async Task FillLocationIfPresent()
    {
        if (await _page.Locator("[data-location]").CountAsync() == 0)
        {
            return;
        }

        // Set the value + lat/long directly (change event only) so the Google Places autocomplete,
        // which listens on focus/input, doesn't fire a billable Places call.
        await _page.EvalOnSelectorAsync(
            "[data-location]",
            "el => { el.value = 'London'; el.dispatchEvent(new Event('change', { bubbles: true })); }");

        if (await _page.Locator("[data-location-lat]").CountAsync() > 0)
        {
            await _page.EvalOnSelectorAsync("[data-location-lat]", "el => el.value = '51.5074'");
        }

        if (await _page.Locator("[data-location-long]").CountAsync() > 0)
        {
            await _page.EvalOnSelectorAsync("[data-location-long]", "el => el.value = '-0.1278'");
        }
    }

    private async Task UploadImageIfPresent()
    {
        if (await _page.Locator("[data-img-input]").CountAsync() == 0)
        {
            return;
        }

        await _page.SetInputFilesAsync("[data-img-input]", TestAssets.GroupImagePath);
        await _page.WaitForFunctionAsync(
            "() => { const el = document.querySelector('[data-img-dataurl]'); return !!el && el.value.length > 0; }");
    }
}
