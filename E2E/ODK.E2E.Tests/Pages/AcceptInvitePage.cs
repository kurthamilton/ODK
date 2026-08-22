using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The Group Squirrel accept-invitation page (<c>/groups/{slug}/accept-invite</c>). An imported member has an
/// account with no password, so this page gives it one and joins the group in the same submit, landing on the
/// login page. It is anonymous: the member it names cannot sign in until they have used it.
/// </summary>
internal class AcceptInvitePage
{
    private readonly IPage _page;

    public AcceptInvitePage(IPage page)
    {
        _page = page;
    }

    /// <summary>Opens the page as the invitation email's link does, carrying the invitation's token.</summary>
    public Task Open(string slug, string inviteToken)
        => _page.Navigate($"/groups/{slug}/accept-invite?token={Uri.EscapeDataString(inviteToken)}");

    /// <summary>
    /// Accepts the invitation: confirms the pre-filled name, sets the password, agrees to the privacy policy
    /// and submits. Returns the URL it landed on, which is the login page carrying a return URL to the group.
    /// </summary>
    public async Task<string> Accept(string password)
    {
        await _page.FillAsync("#Password", password);
        await _page.FillAsync("#ConfirmPassword", password);
        await _page.CheckAsync("#PrivacyPolicy");

        await _page.ClickAsync("button:has-text('Accept invitation')");

        try
        {
            await _page.WaitForURLAsync(
                new Regex("/account/login", RegexOptions.IgnoreCase), new() { Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            var errors = await _page.Locator(
                ".field-validation-error, .text-danger, .validation-summary-errors, .alert").AllInnerTextsAsync();
            throw new InvalidOperationException(
                $"Accepting the invitation did not reach the login page. URL='{_page.Url}'. " +
                $"Validation/alerts=[{string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x)))}].");
        }

        /* The GET response arriving is not the browser having committed the document it carries, and the
           caller's next step navigates to the login page in its own right - which would be cut short by the
           navigation already running. */
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        return _page.Url;
    }

    /// <summary>The address the invitation was sent to, which the page shows but does not let them change.</summary>
    public Task<string> GetEmailAddress()
        => _page.InnerTextAsync("form .form-control-plaintext");

    public Task<string> GetFirstName() => _page.InputValueAsync("#FirstName");

    public Task<string> GetLastName() => _page.InputValueAsync("#LastName");

    /// <summary>
    /// Whether the page is offering the accept form, which it does not when the invitation names somebody who
    /// can already sign in, nor when the token names no outstanding invitation to this group.
    /// </summary>
    public async Task<bool> HasAcceptForm() => await _page.Locator("#Password").CountAsync() > 0;

    /// <summary>Whether the page is asking the visitor to sign in instead of setting a password.</summary>
    public async Task<bool> HasSignInPrompt() => await SignInLink().CountAsync() > 0;

    /// <summary>
    /// Follows the sign-in prompt, landing on the login page with a return URL back to this invitation.
    /// </summary>
    public async Task FollowSignInPrompt()
    {
        await SignInLink().ClickAsync();
        await _page.WaitForURLAsync(new Regex("/account/login", RegexOptions.IgnoreCase));
    }

    /* The prompt's own link, by its data hook rather than its text: the header carries a "Sign in" link on
       every anonymous page, so matching on text finds two - and a presence check on that would have been
       satisfied by the header alone, whether or not the prompt rendered. */
    private ILocator SignInLink() => _page.Locator("[data-invite-signin]");
}
