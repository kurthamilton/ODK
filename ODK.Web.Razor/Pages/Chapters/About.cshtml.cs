using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters
{
    public class AboutModel : ChapterPageModel
    {
        public AboutModel(IRequestCache requestCache) 
            : base(requestCache)
        {
        }

        public void OnGet()
        {
        }
    }
}
