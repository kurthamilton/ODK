using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events.Event;

public class InvitesModel : EventAdminPageModel
{
    public InvitesModel(IEventAdminService eventAdminService)
        : base(eventAdminService)
    {
    }

    public void OnGet()
    {
    }
}