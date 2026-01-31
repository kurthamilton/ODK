using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Members.Member;

public class EventsModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Events;

    public Guid MemberId { get; private set; }

    public void OnGet(Guid memberId)
    {
        MemberId = memberId;
    }
}
