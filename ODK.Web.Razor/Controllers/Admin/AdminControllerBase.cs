using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;

namespace ODK.Web.Razor.Controllers.Admin;

public class AdminControllerBase : OdkControllerBase
{
    protected AdminControllerBase(
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
    }
}