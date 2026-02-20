using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Payments;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Payments;

    public void OnGet()
    {
    }
}
