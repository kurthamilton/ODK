using ODK.Services.Caching;
using ODK.Services.Venues;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class VenueEventsModel : VenueAdminPageModel
{
    public VenueEventsModel(IRequestCache requestCache, IVenueAdminService venueAdminService) 
        : base(requestCache, venueAdminService)
    {
    }

    public void OnGet()
    {
    }
}
