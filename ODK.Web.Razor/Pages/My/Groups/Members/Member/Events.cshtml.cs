namespace ODK.Web.Razor.Pages.My.Groups.Members.Member;

public class EventsModel : OdkGroupAdminPageModel
{
    public Guid MemberId { get; private set; }

    public void OnGet(Guid memberId)
    {
        MemberId = memberId;
    }
}