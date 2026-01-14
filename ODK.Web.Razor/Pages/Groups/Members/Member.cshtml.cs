namespace ODK.Web.Razor.Pages.Groups.Members;

public class MemberModel : OdkPageModel
{
    public Guid MemberId { get; private set; }

    public void OnGet(Guid id)
    {
        MemberId = id;
    }
}