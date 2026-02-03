using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class MembersModel : AdminPageModel
{
    public MembersModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Members;

    public void OnGet()
    {
    }
}