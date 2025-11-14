using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class MissingPaymentsModel : SuperAdminPageModel
{
    public MissingPaymentsModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}