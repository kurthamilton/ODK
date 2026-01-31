using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class EventsModel : AdminPageModel
{
    public EventsModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Events;

    public void OnGet()
    {
    }
}