using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin
{
    public class ErrorsModel : SuperAdminPageModel
    {
        public ErrorsModel(IRequestCache requestCache) 
            : base(requestCache)
        {
        }

        public void OnGet()
        {
        }
    }
}
