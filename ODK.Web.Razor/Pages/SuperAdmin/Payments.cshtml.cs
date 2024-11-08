using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Settings;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class PaymentsModel : SuperAdminPageModel
{
    public PaymentsModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
