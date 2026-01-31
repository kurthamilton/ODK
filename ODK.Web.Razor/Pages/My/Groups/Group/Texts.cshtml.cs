using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class TextsModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Texts;

    public void OnGet()
    {
    }
}
