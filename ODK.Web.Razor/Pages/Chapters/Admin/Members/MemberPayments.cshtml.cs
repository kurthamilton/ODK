using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class MemberPaymentsModel : AdminPageModel
{
    public MemberPaymentsModel()
    {
    }

    public Guid MemberId { get; private set; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Payments;

    public void OnGet(Guid id)
    {
        MemberId = id;
    }
}