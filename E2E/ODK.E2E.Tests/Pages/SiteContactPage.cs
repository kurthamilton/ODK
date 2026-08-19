using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The site's contact page (<c>/contact</c>), which is the same URL on both platforms. Only a visitor
/// sees it: a signed-in member is redirected to their conversations instead.
/// </summary>
internal class SiteContactPage
{
    private const string Form = "form[action='/contact']";

    private readonly IPage _page;

    public SiteContactPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Whether the page currently on screen is the contact page, identified by the form that sends the
    /// message rather than by its heading - the heading is chrome the redirect target has too.
    /// </summary>
    public Task<bool> HasContactForm() => _page.Locator(Form).First.IsVisibleAsync();
}
