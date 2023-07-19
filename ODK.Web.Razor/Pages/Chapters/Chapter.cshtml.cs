using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters
{
    public class ChapterModel : ChapterPageModel
    {
        public ChapterModel(IRequestCache requestCache)
            : base(requestCache)
        {
        }

        public void OnGet()
        {
        }
    }
}
