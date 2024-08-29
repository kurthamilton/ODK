using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Venues;

namespace ODK.Web.Razor.Models.Admin.Events;

public class VenueAdminTabsViewModel
{
    public required Chapter Chapter { get; init; }

    public required PlatformType Platform { get; init; }

    public required Venue Venue { get; init; }
}
