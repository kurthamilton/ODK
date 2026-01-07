using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SuperAdmin;

public class ThemeModel : ChapterSuperAdminPageModel
{
    public ThemeModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
