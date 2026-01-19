namespace ODK.Web.Razor.Pages.SiteAdmin;

public class IssueModel : SiteAdminPageModel
{
    public IssueModel()
    {
    }

    public Guid IssueId { get; private set; }

    public void OnGet(Guid id)
    {
        IssueId = id;
    }
}