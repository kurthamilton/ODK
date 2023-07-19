using ODK.Services.Caching;
using ODK.Services.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members
{
    public class MemberImageModel : MemberAdminPageModel
    {
        public MemberImageModel(IRequestCache requestCache, IMemberAdminService memberAdminService) 
            : base(requestCache, memberAdminService)
        {
        }

        public void OnGet()
        {
        }
    }
}
