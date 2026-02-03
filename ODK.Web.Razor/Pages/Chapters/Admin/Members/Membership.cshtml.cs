using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class MembershipModel : AdminPageModel
{
    public MembershipModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.MembershipSettings;

    public void OnGet()
    {
    }
}