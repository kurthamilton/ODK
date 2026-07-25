using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The member-facing event detail page (Default <c>/groups/{slug}/events/{shortcode}</c>,
/// DrunkenKnitwits <c>/{chapterName}/events/{shortcode}</c>) and the two ways a member RSVPs "yes":
/// the on-page RSVP control, and the "yes" link an event invite email carries.
/// </summary>
internal class EventPage
{
    private readonly IPage _page;

    public EventPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Navigates to the event page and returns the HTTP response status (Playwright doesn't throw on a
    /// 4xx/5xx). Used to assert an event the viewer can't see returns a 404 rather than leaking its
    /// existence.
    /// </summary>
    public async Task<int> GetResponseStatus(string eventPageUrl)
    {
        var response = await _page.GotoAsync(eventPageUrl)
            ?? throw new InvalidOperationException($"No response navigating to '{eventPageUrl}'.");
        return response.Status;
    }

    /// <summary>
    /// Follows the "RSVP yes" link an event invite email would contain
    /// (<c>/.../events/{shortcode}/rsvp</c>). It records "yes" for the authenticated member and redirects
    /// to the event page, so the caller must already be logged in as the member.
    /// </summary>
    public async Task RsvpViaEmailLink(string rsvpUrl)
    {
        await _page.Navigate(rsvpUrl);
        await _page.WaitForLoadStateAsync();

        // The link is [Authorize]d: an unauthenticated hit bounces to login and never records a response.
        if (_page.Url.Contains("/account/login", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"RSVP link redirected to login - the member was not authenticated. URL='{_page.Url}'.");
        }
    }

    /// <summary>
    /// Opens the event page and clicks the "yes" RSVP control, which posts to
    /// <c>/events/{eventId}/rsvp</c> and reloads with the "yes" option marked active.
    /// </summary>
    public async Task RsvpYesOnPage(string eventPageUrl)
    {
        await _page.Navigate(eventPageUrl);

        const string yesButton = "form[action*='/rsvp'] button.event-response-option--yes";
        if (await _page.Locator(yesButton).CountAsync() == 0)
        {
            var body = await _page.InnerTextAsync("body");
            throw new InvalidOperationException(
                $"No RSVP 'yes' control on the event page. URL='{_page.Url}'. " +
                $"Body: {body[..Math.Min(500, body.Length)]}");
        }

        // The response is recorded server-side during this POST, before its post/redirect/get - so the
        // caller can assert the recorded response straight afterwards. We deliberately don't assert the
        // redirected UI state: the form posts to /events/{id}/rsvp and relies on the Referer to redirect
        // back to the event page, which isn't reliable under automation, and the recorded response (the
        // real outcome) is asserted against the database instead.
        await _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync(yesButton),
            r => r.Request.Method == "POST" && r.Url.Contains("/rsvp"));
    }
}
