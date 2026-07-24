using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The Group Squirrel account sign-up wizard (<c>/account/create</c>). The identity step (first name,
/// last name, email) and the location name are client-side required (the wizard won't advance past a
/// page that fails validation); interests are optional.
/// </summary>
internal class SignUpPage
{
    private readonly IPage _page;

    public SignUpPage(IPage page)
    {
        _page = page;
    }

    public async Task SignUpAsync(string firstName, string lastName, string emailAddress)
    {
        await _page.Navigate("/account/create");

        // Step 1 - identity.
        await _page.FillAsync("[data-firstname]", firstName);
        await _page.FillAsync("[data-lastname]", lastName);
        await _page.FillAsync("[data-email]", emailAddress);
        await _page.ClickAsync("#wizard-1 .justify-content-end .btn-primary");

        // Step 2 - location. The name is client-side required, so the wizard won't advance without it.
        // Set the name and lat/long directly, raising only a `change` event - the Google Places
        // autocomplete listens on focus/input, so this satisfies validation without a (billable) Places
        // call. Lat/long matter: CreateAccount only persists a MemberLocation when they're present, and
        // pages like create-group require the member to have one (they throw 404 otherwise).
        await _page.EvalOnSelectorAsync(
            "[data-location]",
            "el => { el.value = 'London'; el.dispatchEvent(new Event('change', { bubbles: true })); }");
        await _page.EvalOnSelectorAsync("[data-location-lat]", "el => el.value = '51.5074'");
        await _page.EvalOnSelectorAsync("[data-location-long]", "el => el.value = '-0.1278'");
        await _page.ClickAsync("#wizard-2 .justify-content-end .btn-primary");

        // Step 3 - interests (optional).
        await _page.ClickAsync("#wizard-3 .justify-content-end .btn-primary");

        // The final "Sign up" button is JS-driven (data-submit="parent").
        await _page.ClickAsync("[data-submit='parent']");

        // Non-activated sign-ups land on the "check your email" page.
        await _page.WaitForURLAsync("**/account/pending");
    }
}