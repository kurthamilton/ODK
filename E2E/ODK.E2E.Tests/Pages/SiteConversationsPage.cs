using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// A member's inbox of threads with the site's admins - Group Squirrel
/// <c>/account/site-conversations</c>, Drunken Knitwits <c>/conversations</c>. The caller passes the
/// platform-correct URL.
/// <para>
/// The start-a-conversation form is rendered either way round: on its own when the member has nothing to
/// list, and behind the "New conversation" button once they have, so <see cref="StartConversation"/>
/// opens the dialog only when it needs to.
/// </para>
/// </summary>
internal class SiteConversationsPage
{
    private const string NewConversationButton = "button:has-text('New conversation')";

    private const string StartForm = "form[action='/site-conversations/start']";

    private readonly IPage _page;

    public SiteConversationsPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Whether the page currently on screen is the conversations inbox, identified by the form that opens
    /// a thread - it is present whether or not the member has conversations to list.
    /// </summary>
    public async Task<bool> HasStartConversationForm()
        => await _page.Locator(StartForm).CountAsync() > 0;

    public Task Open(string url) => _page.Navigate(url);

    /// <summary>
    /// Opens the listed conversation with the given subject and returns its id, read from the address the
    /// link goes to - which is the only place a test can get it without querying the database.
    /// </summary>
    public async Task<Guid> OpenConversation(string subject)
    {
        var from = new Uri(_page.Url).AbsolutePath;

        await _page.ClickAsync($"td a:has-text('{subject}')");

        // A link click, so the address changing is what completes it - and what makes the URL safe to read.
        await _page.WaitForURLAsync(url => new Uri(url).AbsolutePath != from);

        var segments = new Uri(_page.Url).AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return Guid.TryParse(segments.LastOrDefault(), out var id)
            ? id
            : throw new InvalidOperationException(
                $"Conversation '{subject}' did not open a conversation page (at '{_page.Url}').");
    }

    /// <summary>Opens a thread with the site's admins, and returns once the inbox has reloaded.</summary>
    public async Task StartConversation(string subject, string message)
    {
        var form = _page.Locator(StartForm);
        if (!await form.First.IsVisibleAsync())
        {
            await _page.ClickAsync(NewConversationButton);
            await form.First.WaitForAsync();
        }

        await _page.FillAsync($"{StartForm} #Subject", subject);
        await _page.FillAsync($"{StartForm} #Message", message);

        /* Post/Redirect/Get, so the POST response is the 302 and arrives while the redirected GET is still
           in flight - the GET waiter is registered before the click so it cannot be missed, and the network
           is settled afterwards so a caller navigating next is not cut short by a navigation still running. */
        var reloaded = _page.WaitForResponseAsync(
            r => r.Request.Method == "GET" && r.Request.ResourceType == "document");

        await _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync($"{StartForm} button:has-text('Send')"),
            r => r.Request.Method == "POST" && r.Request.ResourceType == "document");

        await reloaded;
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
