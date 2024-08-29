namespace ODK.Web.Razor.Pages.My.Groups.Members;

public class MemberModel : OdkGroupAdminPageModel
{
    public Guid MemberId { get; private set; }

    public void OnGet(Guid memberId)
    {
        MemberId = memberId;
    }
}
