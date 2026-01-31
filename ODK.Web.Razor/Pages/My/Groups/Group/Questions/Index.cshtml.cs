using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Questions;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Questions;

    public void OnGet()
    {
    }
}
