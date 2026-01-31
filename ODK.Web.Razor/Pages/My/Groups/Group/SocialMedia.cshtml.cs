using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class SocialMediaModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.SocialMedia;

    public void OnGet()
    {
    }
}
