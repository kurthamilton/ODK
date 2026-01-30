using Microsoft.AspNetCore.Authorization;
using ODK.Services.Authentication;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;

namespace ODK.Web.Razor.Controllers.Admin;

[Authorize(Roles = OdkRoles.Admin)]
public class AdminControllerBase : OdkControllerBase
{
    protected AdminControllerBase(
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
    }
}