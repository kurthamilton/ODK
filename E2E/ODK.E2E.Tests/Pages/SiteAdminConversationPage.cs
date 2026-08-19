using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The site-admin view of one member's thread (<c>/siteadmin/conversations/{id}</c>), where an admin
/// answers it. Requires the logged-in member to be a site admin (<c>Members.SuperAdmin = 1</c>). The
/// site-admin area is platform-agnostic, so this works against either platform's base URL.
/// </summary>
internal class SiteAdminConversationPage
{
    private readonly IPage _page;

    public SiteAdminConversationPage(IPage page)
    {
        _page = page;
    }

    /// <summary>Sends a reply to the conversation, and returns once the page has reloaded.</summary>
    public async Task Reply(Guid conversationId, string message)
    {
        var replyUrl = $"/siteadmin/conversations/{conversationId}/reply";

        await _page.Navigate($"/siteadmin/conversations/{conversationId}");
        await _page.FillAsync($"form[action='{replyUrl}'] #Message", message);

        /* Post/Redirect/Get, so the POST response is the 302 and arrives while the redirected GET is still
           in flight - the GET waiter is registered before the click so it cannot be missed, and the network
           is settled afterwards so the reply has certainly been committed by the time this returns. */
        var reloaded = _page.WaitForResponseAsync(
            r => r.Request.Method == "GET" && r.Request.ResourceType == "document");

        await _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync($"form[action='{replyUrl}'] button:has-text('Send')"),
            r => r.Request.Method == "POST" && r.Request.ResourceType == "document");

        await reloaded;
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
