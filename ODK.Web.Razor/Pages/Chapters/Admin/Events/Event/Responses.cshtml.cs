using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events.Event;

public class ResponsesModel : EventAdminPageModel
{
    public ResponsesModel(IEventAdminService eventAdminService)
        : base(eventAdminService)
    {
    }

    public void OnGet()
    {
    }
}