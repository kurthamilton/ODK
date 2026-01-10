namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class MemberEventsModel : AdminPageModel
{
    public MemberEventsModel()
    {
    }

    public Guid MemberId { get; private set; }

    public void OnGet(Guid id)
    {
        MemberId = id;
    }
}