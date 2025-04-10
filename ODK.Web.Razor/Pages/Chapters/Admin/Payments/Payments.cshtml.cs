using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Payments;

public class PaymentsModel : AdminPageModel
{
    public PaymentsModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
