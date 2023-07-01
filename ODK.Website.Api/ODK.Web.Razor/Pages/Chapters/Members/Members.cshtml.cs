using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Members
{
    public class MembersModel : ChapterPageModel
    {
        public MembersModel(IRequestCache requestCache)
            : base(requestCache)
        {
        }

        public void OnGet()
        {
        }
    }
}
