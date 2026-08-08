using Microsoft.AspNetCore.Mvc;
using ODK.Data.Core;
using ODK.Data.Core.Events;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class EventsModel : AdminPageModel
{
    public EventsModel()
    {
    }

    public EventAdminFilter Filter { get; private set; } = null!;

    public PageFilter PageFilter { get; private set; } = null!;

    public int PageSize => 20;

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Events;

    // Bind from the query string explicitly: `page` is a reserved Razor Pages route value (the page
    // path), so a plain `int page` parameter would bind that instead of the ?page= query value.
    // "venue" carries the venue's slug, so a filtered URL reads as ?venue=the-oak-tree.
    public void OnGet(
        [FromQuery] string? venue,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1)
    {
        Filter = new EventAdminFilter
        {
            FromDateLocal = fromDate,
            ToDateLocal = toDate,
            VenueSlug = venue
        };

        PageFilter = new PageFilter
        {
            Page = page < 1 ? 1 : page,
            PageSize = PageSize
        };
    }
}