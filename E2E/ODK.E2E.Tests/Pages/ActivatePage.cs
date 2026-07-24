using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The account activation page (<c>/account/activate?token=...</c>), where the member sets their
/// password. On success it redirects to the login page.
/// </summary>
internal class ActivatePage
{
    private readonly IPage _page;

    public ActivatePage(IPage page)
    {
        _page = page;
    }

    public async Task ActivateAsync(string token, string password)
    {
        await _page.Navigate($"/account/activate?token={Uri.EscapeDataString(token)}");

        await _page.FillAsync("#Password", password);
        await _page.FillAsync("#ConfirmPassword", password);
        await _page.ClickAsync("button:has-text('Activate account')");

        await _page.WaitForURLAsync("**/account/login**");
    }
}