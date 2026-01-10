using ODK.Services.Venues;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class VenueEventsModel : VenueAdminPageModel
{
    public VenueEventsModel(IVenueAdminService venueAdminService)
        : base(venueAdminService)
    {
    }

    public void OnGet()
    {
    }
}