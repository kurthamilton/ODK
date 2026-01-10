namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class MemberImageModel : AdminPageModel
{
    public MemberImageModel()
    {
    }

    public Guid MemberId { get; private set; }

    public void OnGet(Guid id)
    {
        MemberId = id;
    }
}