using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// One thread with the site's admins as the member whose conversation it is reads it - Group Squirrel
/// <c>/account/site-conversations/{id}</c>, Drunken Knitwits <c>/conversations/{id}</c>. The caller
/// passes the platform-correct URL.
/// </summary>
internal class SiteConversationPage
{
    private readonly IPage _page;

    public SiteConversationPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// The messages in the thread, oldest first, each as it reads on screen - who it is from, when it was
    /// sent, and what it says.
    /// </summary>
    /// <remarks>
    /// Returned whole rather than picked apart into sender and body: a message is one thing a reader looks
    /// at, and a caller pairing an attribution with the text it sits above is asserting what the page shows
    /// rather than what an element happens to contain. <c>.conversation-message</c> is the class the message
    /// card is styled by, so this needs nothing in the markup that only a test reads.
    /// </remarks>
    public async Task<IReadOnlyList<string>> GetMessages()
    {
        var messages = await _page.Locator(".conversation-message").AllInnerTextsAsync();
        return messages.Select(x => x.Trim()).ToArray();
    }

    public Task Open(string url) => _page.Navigate(url);

    /// <summary>Reloads the thread, so a reply sent from elsewhere shows up.</summary>
    public Task Reload() => _page.ReloadAsync();
}
