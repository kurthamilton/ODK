using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin
{
    public class ErrorLogModel : SuperAdminPageModel
    {
        public ErrorLogModel(IRequestCache requestCache) 
            : base(requestCache)
        {
        }

        public void OnGet()
        {
        }
    }
}
