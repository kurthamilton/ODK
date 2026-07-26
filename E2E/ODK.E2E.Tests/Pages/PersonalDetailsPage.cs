using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The member's personal-details (name) edit page (Default <c>/account</c>, DrunkenKnitwits
/// <c>/{chapterName}/account</c>). The shared form has required First name / Last name fields and posts
/// to the global <c>/Account/PersonalDetails</c> endpoint on both platforms.
/// </summary>
internal class PersonalDetailsPage
{
    private readonly IPage _page;

    public PersonalDetailsPage(IPage page)
    {
        _page = page;
    }

    /// <summary>Whether the given field currently shows a client-side validation error.</summary>
    public async Task<bool> FieldErrorShown(string field)
        => await _page.Locator($"span.field-validation-error[data-valmsg-for='{field}']").CountAsync() > 0;

    /// <summary>
    /// Fills the name fields and submits. Returns true if the update posted, false if it was blocked
    /// client-side (e.g. a required name left blank), so callers can assert either outcome.
    /// </summary>
    public async Task<bool> TryUpdate(string accountUrl, string firstName, string lastName)
    {
        await _page.Navigate(accountUrl);

        await _page.FillAsync("#FirstName", firstName);
        await _page.FillAsync("#LastName", lastName);

        try
        {
            await _page.RunAndWaitForResponseAsync(
                () => _page.ClickAsync("form[action$='PersonalDetails'] button"),
                r => r.Request.Method == "POST" && r.Url.Contains("/personaldetails", StringComparison.OrdinalIgnoreCase),
                new() { Timeout = 8000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
