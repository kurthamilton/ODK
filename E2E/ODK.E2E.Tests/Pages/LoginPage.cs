using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The Group Squirrel login page (<c>/account/login</c>). On success it redirects away from the
/// login page (to the return URL or home).
/// </summary>
internal class LoginPage
{
    private readonly IPage _page;

    public LoginPage(IPage page)
    {
        _page = page;
    }

    public async Task LogIn(string emailAddress, string password)
    {
        await _page.Navigate("/account/login");

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

        // A failed login redirects back to the login page; success redirects away from it.
        await _page.WaitForURLAsync(
            url => !url.Contains("/account/login", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Attempts a login and returns whether it succeeded (redirected off the login page). Used to verify a
    /// password change - run it on a fresh, unauthenticated context.
    /// </summary>
    public async Task<bool> TryLogIn(string emailAddress, string password)
    {
        await _page.Navigate("/account/login");

        await _page.FillAsync("#Email", emailAddress);
        await _page.FillAsync("#Password", password);
        await _page.ClickAsync("button:has-text('Sign in')");

        try
        {
            await _page.WaitForURLAsync(url => !url.Contains("/account/login"), new() { Timeout = 8000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}