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

    /// <summary>
    /// Signs up (= joins) filling the personal details and the given chapter-property answers, then
    /// submits. Returns true if it reached the "check your email" page, false if it was blocked
    /// client-side (e.g. a required property was left unanswered).
    /// </summary>
    public async Task<bool> TryJoinWithProperties(
        string chapterShortName, string firstName, string lastName, string email,
        IReadOnlyDictionary<Guid, string> propertyAnswers)
    {
        await _page.Navigate($"/{chapterShortName}/account/join");

        await _page.FillAsync("[data-firstname]", firstName);
        await _page.FillAsync("[data-lastname]", lastName);
        await _page.FillAsync("[data-email]", email);

        await FillLocationIfPresent();
        await CheckPrivacyPolicyIfPresent();
        await UploadImageIfPresent();

        foreach (var (chapterPropertyId, value) in propertyAnswers)
        {
            await _page.FillChapterProperty(chapterPropertyId, value);
        }

        await _page.ClickAsync("button:has-text('Create')");

        try
        {
            await _page.WaitForURLAsync(
                new Regex("/account/pending", RegexOptions.IgnoreCase), new() { Timeout = 10000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
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

        await Submit(new Regex("/account/pending", RegexOptions.IgnoreCase), "/account/pending");
    }

    /// <summary>
    /// Opens the join page as an invitation link does, carrying the invitation's token.
    /// </summary>
    public Task OpenInvitation(string chapterShortName, string inviteToken)
        => _page.Navigate($"/{chapterShortName}/account/join?token={Uri.EscapeDataString(inviteToken)}");

    public Task<string> GetEmailAddress() => _page.InputValueAsync("[data-email]");

    public Task<string> GetFirstName() => _page.InputValueAsync("[data-firstname]");

    public Task<string> GetLastName() => _page.InputValueAsync("[data-lastname]");

    /// <summary>
    /// Whether the page is offering the sign-up form, which it does not when the invitation names someone who
    /// already has an account.
    /// </summary>
    public async Task<bool> HasSignUpForm() => await _page.Locator("[data-email]").CountAsync() > 0;

    /// <summary>
    /// Whether the page is asking the visitor to sign in instead of signing up.
    /// </summary>
    public async Task<bool> HasSignInPrompt() => await SignInLink().CountAsync() > 0;

    /// <summary>
    /// Follows the sign-in prompt, landing on the chapter login page with a return URL back to this
    /// invitation.
    /// </summary>
    public async Task FollowSignInPrompt()
    {
        await SignInLink().ClickAsync();
        await _page.WaitForURLAsync(new Regex("/account/login", RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// Submits the invited sign-up, optionally replacing the address the invitation pre-filled, and returns
    /// the URL it landed on: the activate page when the invitation stood in for the activation email, or the
    /// "check your email" page when it did not.
    /// </summary>
    public async Task<string> AcceptInvitation(string? replacementEmailAddress = null)
    {
        if (replacementEmailAddress != null)
        {
            await _page.FillAsync("[data-email]", replacementEmailAddress);
        }

        await FillLocationIfPresent();
        await CheckPrivacyPolicyIfPresent();
        await UploadImageIfPresent();

        await _page.ClickAsync("button:has-text('Create')");

        await Submit(
            new Regex("/account/(activate|pending)", RegexOptions.IgnoreCase),
            "/account/activate or /account/pending");

        return _page.Url;
    }

    /// <summary>
    /// Submits the join form as a signed-in member, for whom this page joins the group rather than creating
    /// an account - so it asks only the group's own questions and lands on the group home.
    /// </summary>
    public async Task JoinAsSignedInMember(string chapterShortName)
    {
        await _page.ClickAsync("button:has-text('Join')");

        await Submit(new Regex($"/{chapterShortName}/?$", RegexOptions.IgnoreCase), $"/{chapterShortName}");
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

    /* The prompt's own link, by its data hook rather than its text: the header carries a "Sign in" link on
       every anonymous page, so matching on text finds two - and a presence check on that would have been
       satisfied by the header alone, whether or not the prompt rendered. */
    private ILocator SignInLink() => _page.Locator("[data-invite-signin]");

    /// <summary>
    /// Waits for the navigation a submitted form causes. If the submit was blocked - a required field
    /// client-side, or the server re-rendering with errors - the page never navigates, so surface what the
    /// page is saying instead of a bare timeout.
    /// </summary>
    private async Task Submit(Regex expectedUrl, string expectedDescription)
    {
        try
        {
            await _page.WaitForURLAsync(expectedUrl, new() { Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            var errors = await _page.Locator(
                ".field-validation-error, .text-danger, .validation-summary-errors, .alert").AllInnerTextsAsync();
            var body = await _page.InnerTextAsync("body");
            throw new InvalidOperationException(
                $"DrunkenKnitwits join did not reach {expectedDescription}. URL='{_page.Url}'. " +
                $"Validation/alerts=[{string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x)))}]. " +
                $"Body: {body[..Math.Min(500, body.Length)]}");
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
