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

        await _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync("form[action$='Password/Forgotten'] button"),
            r => r.Request.Method == "POST" && r.Request.ResourceType == "document");
        await _page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Sets a new password from a reset link, and reports whether the app accepted it. The form posts back
    /// to itself, so success redirects to the login page and failure re-renders the form - which is what
    /// distinguishes the two without reading a message.
    /// </summary>
    public async Task<bool> TryResetPassword(string passwordResetUrl, string newPassword)
    {
        await _page.Navigate(passwordResetUrl);
        await _page.FillAsync("#NewPassword", newPassword);
        await _page.FillAsync("#ConfirmNewPassword", newPassword);

        await _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync("button:has-text('Reset')"),
            r => r.Request.Method == "POST" && r.Request.ResourceType == "document");
        await _page.WaitForLoadStateAsync();

        return _page.Url.Contains("/account/login", StringComparison.OrdinalIgnoreCase);
    }
}
