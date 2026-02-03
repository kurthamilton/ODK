using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Events.Event;

public class CommentsModel : OdkGroupAdminPageModel
{
    public Guid EventId { get; private set; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Events;

    public void OnGet(Guid eventId)
    {
        EventId = eventId;
    }
}