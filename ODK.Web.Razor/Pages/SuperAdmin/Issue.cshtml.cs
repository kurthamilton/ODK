using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class IssueModel : SuperAdminPageModel
{
    public IssueModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public Guid IssueId { get; private set; }

    public void OnGet(Guid id)
    {
        IssueId = id;
    }
}
