using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The admin event-settings page (Default <c>/my/groups/{chapterId}/events/settings</c>,
/// DrunkenKnitwits <c>/{chapterName}/admin/events/settings</c>). Sets the chapter's default event day of
/// week (a SlimSelect-enhanced dropdown of .NET <c>DayOfWeek</c>) and default start time (a plain
/// <c>HH:mm</c> text field), which together seed the pre-populated create-event date. The shared form
/// posts to a controller and redirects back to the settings page with a confirmation.
/// </summary>
internal class EventSettingsPage
{
    private readonly IPage _page;

    public EventSettingsPage(IPage page)
    {
        _page = page;
    }

    public async Task SetDefaults(string settingsUrl, DayOfWeek defaultDayOfWeek, string defaultStartTime)
    {
        await _page.Navigate(settingsUrl);

        await _page.SetEnhancedSelect("#DefaultDayOfWeek", ((int)defaultDayOfWeek).ToString());
        await _page.FillAsync("#DefaultStartTime", defaultStartTime);

        try
        {
            await _page.RunAndWaitForResponseAsync(
                () => _page.ClickAsync("button:has-text('Update')"),
                r => r.Request.Method == "POST" && r.Url.Contains("/events/settings"),
                new() { Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            var errors = await _page.Locator(
                ".field-validation-error, .text-danger, .validation-summary-errors, .alert").AllInnerTextsAsync();
            throw new InvalidOperationException(
                $"Update event settings did not post. URL='{_page.Url}'. " +
                $"Validation/alerts=[{string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x)))}].");
        }

        // The controller redirects back to the settings page with a feedback alert - wait for it so a
        // caller reading the settings straight after sees the committed values.
        await _page.WaitForLoadStateAsync();
    }
}
