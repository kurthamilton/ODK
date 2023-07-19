using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Account
{
    public class ForgottenPasswordModel : OdkPageModel
    {
        public ForgottenPasswordModel(IRequestCache requestCache)
            : base(requestCache)
        {
        }

        public void OnGet()
        {
        }
    }
}
