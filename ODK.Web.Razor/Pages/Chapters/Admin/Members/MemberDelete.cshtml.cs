namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class MemberDeleteModel : AdminPageModel
{
    public MemberDeleteModel()
    {
    }

    public Guid MemberId { get; private set; }

    public void OnGet(Guid id)
    {
        MemberId = id;
    }
}