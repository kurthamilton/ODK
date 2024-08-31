using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class MembershipModel : AdminPageModel
{
    public MembershipModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
