using ODK.Core.Chapters;
using ODK.Core.Settings;

namespace ODK.Web.Razor.Models.Admin.Venues;

public class VenueCreateViewModel
{
    public required Chapter Chapter { get; set; }

    public required SiteSettings Settings { get; set; }
}
