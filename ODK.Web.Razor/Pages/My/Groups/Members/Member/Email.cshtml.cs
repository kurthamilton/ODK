namespace ODK.Web.Razor.Pages.My.Groups.Members.Member;

public class EmailModel : OdkGroupAdminPageModel
{
    public Guid MemberId { get; private set; }

    public void OnGet(Guid memberId)
    {
        MemberId = memberId;
    }
}