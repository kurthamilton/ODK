using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The DrunkenKnitwits chapter activation page (<c>/{chapterName}/account/activate?token=...</c>),
/// where the member sets their password. On success it redirects to the chapter login page.
/// </summary>
internal class DrunkenKnitwitsActivatePage
{
    private readonly IPage _page;

    public DrunkenKnitwitsActivatePage(IPage page)
    {
        _page = page;
    }

    public async Task Activate(string chapterShortName, string token, string password)
    {
        await _page.Navigate($"/{chapterShortName}/account/activate?token={Uri.EscapeDataString(token)}");

        await _page.FillAsync("#Password", password);
        await _page.FillAsync("#ConfirmPassword", password);
        await _page.ClickAsync("button:has-text('Activate account')");

        // Success redirects to the chapter login page (/{chapterName}/Account/Login), which is
        // PascalCased - match case-insensitively rather than the lowercase glob.
        await _page.WaitForURLAsync(new Regex("/account/login", RegexOptions.IgnoreCase));
    }
}
