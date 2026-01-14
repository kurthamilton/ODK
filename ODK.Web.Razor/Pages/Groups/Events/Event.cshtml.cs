namespace ODK.Web.Razor.Pages.Groups.Events;

public class EventModel : OdkPageModel
{
    public Guid EventId { get; private set; }

    public void OnGet(Guid id)
    {
        EventId = id;
    }
}