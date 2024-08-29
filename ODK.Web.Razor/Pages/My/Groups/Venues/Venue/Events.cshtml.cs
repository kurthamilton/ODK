namespace ODK.Web.Razor.Pages.My.Groups.Venues.Venue;

public class EventsModel : OdkGroupAdminPageModel
{
    public Guid VenueId { get; private set; }

    public void OnGet(Guid venueId)
    {
        VenueId = venueId;
    }
}
