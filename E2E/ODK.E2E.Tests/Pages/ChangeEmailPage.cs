using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The member's change-email flow (Default form on <c>/account/emails</c>, DrunkenKnitwits page
/// <c>/{chapterName}/account/email/change</c>). Changing email is a two-step, confirm-token flow:
/// requesting the change stores a token and emails the new address; the email only changes when the
/// member follows the confirmation link.
/// </summary>
internal class ChangeEmailPage
{
    private readonly IPage _page;

    public ChangeEmailPage(IPage page)
    {
        _page = page;
    }

    /// <summary>Follows the confirmation link (as the logged-in member) to complete a pending change.</summary>
    public async Task Confirm(string confirmUrl)
    {
        await _page.Navigate(confirmUrl);
        await _page.WaitForLoadStateAsync();
    }

    /// <summary>Requests an email change to <paramref name="newEmail"/> (does not itself change the email).</summary>
    public async Task RequestChange(string changeEmailUrl, string newEmail)
    {
        await _page.Navigate(changeEmailUrl);

        await _page.FillAsync("#Email", newEmail);
        await _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync("form[action$='/email/change'] button"),
            r => r.Request.Method == "POST" && r.Url.Contains("/email/change"));
        await _page.WaitForLoadStateAsync();
    }
}
