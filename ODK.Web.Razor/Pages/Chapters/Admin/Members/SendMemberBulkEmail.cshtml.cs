using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class SendMemberBulkEmailModel : AdminPageModel
{
    public SendMemberBulkEmailModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.BulkEmail;

    public void OnGet()
    {
    }
}