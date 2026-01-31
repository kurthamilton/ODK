using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class DeleteModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Publish;

    public void OnGet()
    {
    }
}
