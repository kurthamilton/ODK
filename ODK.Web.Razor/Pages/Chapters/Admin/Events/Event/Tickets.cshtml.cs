using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events.Event;

public class TicketsModel : EventAdminPageModel
{
    public TicketsModel(IEventAdminService eventAdminService)
        : base(eventAdminService)
    {
    }

    public void OnGet()
    {
    }
}