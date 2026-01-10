using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class EventTicketsModel : EventAdminPageModel
{
    public EventTicketsModel(IEventAdminService eventAdminService)
        : base(eventAdminService)
    {
    }

    public void OnGet()
    {
    }
}