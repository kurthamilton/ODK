namespace ODK.Web.Razor.Pages.Account;

public class IssueModel : OdkSiteAccountPageModel
{
    public Guid IssueId { get; private set; }

    public void OnGet(Guid id)
    {
        IssueId = id;
    }
}