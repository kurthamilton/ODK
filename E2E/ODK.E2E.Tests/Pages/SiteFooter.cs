using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The site footer, which every layout renders on both platforms (<c>_SiteFooter</c> on Group Squirrel,
/// <c>_OdkFooter</c> on Drunken Knitwits). Its Contact link is the way in to contacting the site, and
/// where it goes depends on who is reading it - the contact form for a visitor, the member's own
/// conversations once they are signed in.
/// </summary>
internal class SiteFooter
{
    /* Scoped to the footer landmark, of which a page renders exactly one: the group menu carries a Contact
       link too, so the text alone matches twice. Deliberately not matched on its href - where the link goes
       is the thing under test, so putting the destination in the selector would assert it into existence. */
    private const string ContactLink = "footer a:has-text('Contact')";

    private readonly IPage _page;

    public SiteFooter(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Clicks the footer's Contact link and returns once the page it opens has loaded.
    /// </summary>
    /// <remarks>
    /// A link click is a navigation rather than a submit, so what completes it is the address changing away
    /// from the page the footer was read on - which is also what makes <see cref="IPage.Url"/> safe to read
    /// afterwards, the document response arriving being no guarantee the browser has committed it. Where the
    /// link goes is deliberately not waited on: that is the thing under test.
    /// </remarks>
    public async Task ClickContact()
    {
        // The cookie banner is fixed over the footer, so a visitor gets it out of the way before they can
        // use anything down here.
        await _page.DismissCookieBanner();

        var from = new Uri(_page.Url).AbsolutePath;

        await _page.ClickAsync(ContactLink);

        await _page.WaitForURLAsync(url => new Uri(url).AbsolutePath != from);
    }
}
