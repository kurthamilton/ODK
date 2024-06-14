using Microsoft.AspNetCore.Authorization;
using ODK.Services.Caching;
using ODK.Web.Razor.Pages.Chapters.Admin;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin;

[Authorize(Roles = "SuperAdmin")]
public abstract class SuperAdminPageModel : AdminPageModel
{
    protected SuperAdminPageModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }
}
