using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class GeolocationModel : SiteAdminPageModel
{
    // Not Path: OdkPageModel already uses that for the canonical URL.
    public string? BrowsePath { get; private set; }

    public void OnGet([FromQuery] string? path) => BrowsePath = path;
}
