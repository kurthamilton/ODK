using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The admin create-event page (Default <c>/my/groups/{chapterId}/events/new</c>, DrunkenKnitwits
/// <c>/{chapterName}/admin/events/create</c>). The shared form is identical on both platforms; the
/// caller passes the platform-correct create path. The required fields are Name, Venue (a
/// SlimSelect-enhanced dropdown) and Date (a flatpickr date+time input). On success the app redirects
/// to the events list.
/// </summary>
internal class EventAdminPage
{
    private readonly IPage _page;

    public EventAdminPage(IPage page)
    {
        _page = page;
    }

    public async Task CreateEvent(
        string createUrl, string name, Guid venueId, string date, bool draft = false, int? attendeeLimit = null)
    {
        await _page.Navigate(createUrl);

        await _page.FillAsync("#Name", name);
        await _page.SetEnhancedSelect("#Venue", venueId.ToString());
        await _page.SetDatePicker("#Date", date);

        if (attendeeLimit != null)
        {
            await _page.FillAsync("#AttendeeLimit", attendeeLimit.Value.ToString());
        }

        // "Create" publishes; "Save as draft" (name=draft, value=true) leaves the event unpublished.
        await _page.ClickAsync(draft ? "button[name='draft'][value='true']" : "button:has-text('Create')");

        // Success redirects to the events list (path ends '/events'); the create page ends '/new' or
        // '/create'. Staying put means a re-rendered validation error - surface it.
        try
        {
            await _page.WaitForURLAsync(
                url => new Uri(url).AbsolutePath.TrimEnd('/').EndsWith("/events"),
                new() { Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            var errors = await _page.Locator(
                ".field-validation-error, .text-danger, .validation-summary-errors, .alert").AllInnerTextsAsync();
            var body = await _page.InnerTextAsync("body");
            throw new InvalidOperationException(
                $"Create event did not redirect to the events list. URL='{_page.Url}'. " +
                $"Validation/alerts=[{string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x)))}]. " +
                $"Body: {body[..Math.Min(500, body.Length)]}");
        }
    }

    /// <summary>
    /// Opens the create-event page and returns the value the Date field is pre-populated with (in the
    /// display format <c>dd/MM/yyyy HH:mm</c>). The app defaults this to the next instance of the
    /// chapter's default event day/time.
    /// </summary>
    public async Task<string> GetPrepopulatedDate(string createUrl)
    {
        await _page.Navigate(createUrl);
        return await _page.InputValueAsync("#Date");
    }

    /// <summary>
    /// Opens the create-event page, sets the Date to <paramref name="dateInPostFormat"/> (dd/MM/yyyy HH:mm)
    /// and returns both the underlying posted value (<c>#Date</c>, always the fixed format) and the visible
    /// localised display (flatpickr's altInput). Used to verify locale-based date-picker display.
    /// </summary>
    public async Task<(string Value, string Display)> SetDateAndReadPicker(string createUrl, string dateInPostFormat)
    {
        await _page.Navigate(createUrl);
        await _page.SetDatePicker("#Date", dateInPostFormat);

        var value = await _page.InputValueAsync("#Date");
        var display = await _page.EvaluateAsync<string>(
            "() => document.querySelector('#Date')._flatpickr.altInput.value");
        return (value, display);
    }
}
