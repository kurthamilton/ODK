namespace ODK.Web.Razor.Pages.My.Groups.Events;

public class EventModel : OdkGroupAdminPageModel
{
    public Guid EventId { get; private set; }

    public void OnGet(Guid eventId)
    {
        EventId = eventId;
    }
}
