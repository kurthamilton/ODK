using Microsoft.AspNetCore.Mvc;
using ODK.Web.Common.Extensions;

namespace ODK.Web.Views
{
    public abstract class OdkViewComponentBase : ViewComponent
    {
        protected Guid? GetMemberId()
        {
            return HttpContext.User.MemberId();
        }
    }
}
