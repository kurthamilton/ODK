using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SuperAdmin;

public class ReconciliationsModel : ChapterSuperAdminPageModel
{
    public ReconciliationsModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
