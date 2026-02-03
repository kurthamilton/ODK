using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Members.Member;

public class ConversationsModel : OdkGroupAdminPageModel
{
    public Guid MemberId { get; private set; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Conversations;

    public void OnGet(Guid memberId)
    {
        MemberId = memberId;
    }
}
