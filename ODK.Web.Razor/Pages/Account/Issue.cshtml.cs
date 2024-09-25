namespace ODK.Web.Razor.Pages.Account;

public class IssueModel : OdkPageModel
{
    public Guid IssueId { get; private set; }

    public void OnGet(Guid id)
    {
        IssueId = id;
    }
}
