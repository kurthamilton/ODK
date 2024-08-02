using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members
{
    public class MemberAvatarsModel : AdminPageModel
    {
        public MemberAvatarsModel(IRequestCache requestCache) 
            : base(requestCache)
        {
        }

        public void OnGet()
        {
        }
    }
}
