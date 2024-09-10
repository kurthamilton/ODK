using ODK.Services.Caching;
using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class EventResponsesModel : EventAdminPageModel
{
    public EventResponsesModel(IRequestCache requestCache, IEventAdminService eventAdminService) 
        : base(requestCache, eventAdminService)
    {
    }

    public void OnGet()
    {
    }
}
