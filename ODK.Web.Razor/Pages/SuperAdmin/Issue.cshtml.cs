namespace ODK.Web.Razor.Pages.SuperAdmin;

public class IssueModel : SuperAdminPageModel
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