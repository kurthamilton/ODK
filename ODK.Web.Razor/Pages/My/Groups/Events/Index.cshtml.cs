using Microsoft.AspNetCore.Mvc;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Events;

public class IndexModel : OdkGroupAdminPageModel
{
    public DateTime? FromDate { get; private set; }

    public int Page { get; private set; }

    public int PageSize => 20;

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Events;

    public DateTime? ToDate { get; private set; }

    public Guid? VenueId { get; private set; }

    // Bind from the query string explicitly: `page` is a reserved Razor Pages route value (the page
    // path), so a plain `int page` parameter would bind that instead of the ?page= query value.
    public void OnGet(
        [FromQuery] Guid? venueId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1)
    {
        FromDate = fromDate;
        Page = page < 1 ? 1 : page;
        ToDate = toDate;
        VenueId = venueId;
    }
}
