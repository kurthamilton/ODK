using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using ODK.Core.Chapters;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Authentication;

public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly IUrlHelperFactory _helper;
    private readonly IActionContextAccessor _accessor;

    public CustomCookieAuthenticationEvents(IUrlHelperFactory helper, IActionContextAccessor accessor)
    {
        _helper = helper;
        _accessor = accessor;
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        var routeData = context.Request.HttpContext.GetRouteData();
        string? chapterName = routeData.Values["chapterName"]?.ToString();

        string? redirectUri = context.Request.Path.Value;
        if (!string.IsNullOrEmpty(context.Request.QueryString.Value))
        {
            redirectUri += "?" + context.Request.QueryString.Value;
        }

        var fakeChapter = chapterName != null
            ? new Chapter { Name = chapterName }
            : null;
        context.RedirectUri = OdkRoutes.Account.Login(fakeChapter);
        if (!string.IsNullOrEmpty(redirectUri))
        {
            context.RedirectUri += $"?{context.Options.ReturnUrlParameter}={HttpUtility.UrlEncode(redirectUri)}";
        }

        return base.RedirectToLogin(context);
    }
}
