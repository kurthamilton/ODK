using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events.Event;

public class CommentsModel : EventAdminPageModel
{
    public CommentsModel(IEventAdminService eventAdminService)
        : base(eventAdminService)
    {
    }

    public void OnGet()
    {
    }
}