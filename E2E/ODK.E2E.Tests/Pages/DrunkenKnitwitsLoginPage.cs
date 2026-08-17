using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The DrunkenKnitwits chapter login page (<c>/{chapterName}/account/login</c>). On success it
/// redirects away from the login page.
/// </summary>
internal class DrunkenKnitwitsLoginPage
{
    private readonly IPage _page;

    public DrunkenKnitwitsLoginPage(IPage page)
    {
        _page = page;
    }

    public async Task LogIn(string chapterShortName, string emailAddress, string password)
    {
        await _page.Navigate($"/{chapterShortName}/account/login");

        await LogInOnCurrentPage(emailAddress, password);
    }

    /// <summary>
    /// Logs in using the login page already on screen, rather than navigating to it - which is what keeps the
    /// return URL a link carried into the page, so the member lands back where they were sent from.
    /// </summary>
    public async Task LogInOnCurrentPage(string emailAddress, string password)
    {
        await _page.FillAsync("#Email", emailAddress);
        await _page.FillAsync("#Password", password);
        await _page.ClickAsync("button:has-text('Sign in')");

        // A failed login redirects back to the login page; success redirects away from it. The
        // chapter login URL is PascalCased (/{chapterName}/Account/Login), so compare case-insensitively -
        // a case-sensitive lowercase check would treat the failed-login page as "away" and pass wrongly.
        await _page.WaitForURLAsync(
            url => !url.Contains("/account/login", StringComparison.OrdinalIgnoreCase));
    }
}
