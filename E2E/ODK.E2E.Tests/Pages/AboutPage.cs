using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The public About page (<c>/about</c>), which lists the site FAQ for the current platform. The page
/// 404s when the platform has no questions, so <see cref="Open"/> returns the response status rather
/// than assuming a page was rendered.
/// </summary>
internal class AboutPage
{
    private readonly IPage _page;

    public AboutPage(IPage page)
    {
        _page = page;
    }

    public async Task<bool> HasFooterLink()
    {
        await Open();
        return await _page.Locator("footer a[href='/about']").CountAsync() > 0;
    }

    public async Task<bool> HasQuestion(string question, string answer)
    {
        await Open();

        var card = _page.Locator(".column-item").Filter(new() { HasText = question });
        if (await card.CountAsync() == 0)
        {
            return false;
        }

        return await card.Filter(new() { HasText = answer }).CountAsync() > 0;
    }

    public async Task<int> Open()
    {
        var response = await _page.Navigate("/about");
        return response?.Status ?? 0;
    }

    /// <summary>
    /// The questions in the order they are rendered, so a reorder in the admin UI can be asserted here.
    /// </summary>
    public async Task<IReadOnlyList<string>> QuestionsInOrder()
    {
        await Open();
        return await _page.Locator(".column-item .card-header").AllInnerTextsAsync();
    }
}
