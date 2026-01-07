using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class TextModel : AdminPageModel
{
    public TextModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
