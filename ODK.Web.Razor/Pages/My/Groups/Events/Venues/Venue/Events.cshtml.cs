namespace ODK.Web.Razor.Pages.My.Groups.Events.Venues.Venue;

public class EventsModel : OdkGroupAdminPageModel
{
    public Guid VenueId { get; private set; }

    public void OnGet(Guid venueId)
    {
        VenueId = venueId;
    }
}
