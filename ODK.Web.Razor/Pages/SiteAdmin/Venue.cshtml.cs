namespace ODK.Web.Razor.Pages.SiteAdmin;

public class VenueModel : SiteAdminPageModel
{
    public Guid VenueId { get; private set; }

    public void OnGet(Guid id)
    {
        VenueId = id;
    }
}
