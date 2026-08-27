using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The member's change-password page (Default <c>/account/password/change</c>, DrunkenKnitwits
/// <c>/{chapterName}/account/password/change</c>). The shared form posts to the global
/// <c>/Account/Password/Change</c> endpoint on both platforms. Success and failure both redirect back to
/// the form, so callers verify the outcome by logging in with the old/new password.
/// </summary>
internal class ChangePasswordPage
{
    private readonly IPage _page;

    public ChangePasswordPage(IPage page)
    {
        _page = page;
    }

    public async Task Change(string changePasswordUrl, string currentPassword, string newPassword)
    {
        await _page.Navigate(changePasswordUrl);

        await _page.FillAsync("#CurrentPassword", currentPassword);
        await _page.FillAsync("#NewPassword", newPassword);
        await _page.FillAsync("#ConfirmNewPassword", newPassword);

        await _page.RunAndWaitForDocument(() => _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync("form[action$='Password/Change'] button"),
            r => r.Request.Method == "POST" && r.Url.Contains("/password/change", StringComparison.OrdinalIgnoreCase)));
    }
}
