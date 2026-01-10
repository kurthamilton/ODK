using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class EventInvitesModel : EventAdminPageModel
{
    public EventInvitesModel(IEventAdminService eventAdminService)
        : base(eventAdminService)
    {
    }

    public void OnGet()
    {
    }
}