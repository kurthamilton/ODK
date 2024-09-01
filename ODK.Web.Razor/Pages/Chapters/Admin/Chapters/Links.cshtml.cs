using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class LinksModel : AdminPageModel
{
    public LinksModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
