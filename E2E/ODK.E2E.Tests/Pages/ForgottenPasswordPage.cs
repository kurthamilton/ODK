using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The forgotten-password request form and the reset form the emailed link lands on. Both exist on each
/// platform under that platform's account tree, so the URLs come from <see cref="AccountRoutes"/>.
/// </summary>
/// <remarks>
/// The request form always reports success, whether or not the address is registered, so that nobody can
/// learn from the response which addresses have accounts. A test therefore reads the token from the
/// database rather than inferring anything from the page.
/// </remarks>
internal class ForgottenPasswordPage
{
    private readonly IPage _page;

    public ForgottenPasswordPage(IPage page)
    {
        _page = page;
    }

    /// <summary>Asks for a reset link for the address.</summary>
    public async Task RequestReset(string forgottenPasswordUrl, string emailAddress)
    {
        await _page.Navigate(forgottenPasswordUrl);
        await _page.FillAsync("#EmailAddress", emailAddress);

        /* Post/Redirect/Get onto the same URL, so there is no address change to wait for - settle the
           network instead, or the caller's next navigation collides with the redirect still in flight. */
        var rendered = _page.WaitForResponseAsync(
            r => r.Request.Method == "GET" && r.Request.ResourceType == "document");

        await _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync("form[action$='Password/Forgotten'] button"),
            r => r.Request.Method == "POST" && r.Request.ResourceType == "document");

        await rendered;
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Sets a new password from a reset link, and reports whether the app accepted it.
    /// </summary>
    /// <remarks>
    /// The outcomes are told apart by where the browser ends up, because only one of them navigates: success
    /// redirects to the login page, failure re-renders the form in place. So this waits for the URL to become
    /// the login page and reads a timeout as refusal - the same shape as <see cref="LoginPage.TryLogIn"/>.
    /// Waiting for a document GET unconditionally would hang on the failure branch, which issues none.
    /// </remarks>
    public async Task<bool> TryResetPassword(string passwordResetUrl, string newPassword)
    {
        var response = await _page.Navigate(passwordResetUrl);

        /* Checked rather than left to the first FillAsync: Playwright does not throw on 4xx, so a page that
           did not render at all fails thirty seconds later as a locator timeout naming a field, which says
           nothing about why. A refused reset is a rendered form - never a status - so a bad status here is
           always the page failing to load. */
        if (response?.Ok != true)
        {
            throw new InvalidOperationException(
                $"The reset page at '{passwordResetUrl}' returned {response?.Status}, so its form never rendered.");
        }

        await _page.FillAsync("#NewPassword", newPassword);
        await _page.FillAsync("#ConfirmNewPassword", newPassword);

        await _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync("button:has-text('Reset')"),
            r => r.Request.Method == "POST" && r.Request.ResourceType == "document");

        try
        {
            await _page.WaitForURLAsync(
                url => url.Contains("/account/login", StringComparison.OrdinalIgnoreCase),
                new() { Timeout = 8000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
