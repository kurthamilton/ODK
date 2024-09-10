namespace ODK.Web.Razor.Pages.My.Groups.Events.Event;

public class ResponsesModel : OdkGroupAdminPageModel
{
    public Guid EventId { get; private set; }

    public void OnGet(Guid eventId)
    {
        EventId = eventId;
    }
}
