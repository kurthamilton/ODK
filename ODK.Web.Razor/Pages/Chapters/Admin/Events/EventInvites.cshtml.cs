using ODK.Services.Caching;
using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class EventInvitesModel : EventAdminPageModel
{
    public EventInvitesModel(IRequestCache requestCache, IEventAdminService eventAdminService) 
        : base(requestCache, eventAdminService)
    {
    }

    public void OnGet()
    {
    }
}
