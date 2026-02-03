using System.Web;
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
        string? redirectUri = context.Request.Path.Value;
        if (!string.IsNullOrEmpty(context.Request.QueryString.Value))
        {
            redirectUri += "?" + context.Request.QueryString.Value;
        }

        context.RedirectUri = _odkRoutes.Account.Login(_requestStore.ChapterOrDefault);
        if (!string.IsNullOrEmpty(redirectUri))
        {
            context.RedirectUri += $"?{context.Options.ReturnUrlParameter}={HttpUtility.UrlEncode(redirectUri)}";
        }

        return base.RedirectToLogin(context);
    }
}
