using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class AdminMembersModel : AdminPageModel
{
    public AdminMembersModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
