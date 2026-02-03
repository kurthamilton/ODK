using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Payments;

public class PaymentsModel : AdminPageModel
{
    public PaymentsModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Payments;

    public void OnGet()
    {
    }
}