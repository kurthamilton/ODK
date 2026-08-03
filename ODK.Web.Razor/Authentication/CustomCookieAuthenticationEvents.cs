using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;

namespace ODK.Web.Razor.Authentication;

public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly IOdkRoutes _odkRoutes;
    private readonly IRequestStore _requestStore;

    public CustomCookieAuthenticationEvents(
        IOdkRoutes odkRoutes,
        IRequestStore requestStore)
    {
        _odkRoutes = odkRoutes;
        _requestStore = requestStore;
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        var returnUrl = $"{context.Request.Path}{context.Request.QueryString}";
        context.RedirectUri = _odkRoutes.Account.Login(_requestStore.ChapterOrDefault, returnUrl);
        return base.RedirectToLogin(context);
    }
}
