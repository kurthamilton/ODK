using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Controllers;

namespace ODK.Web.Razor.Tests.Controllers;

[Parallelizable]
public static class OdkControllerBaseTests
{
    [Test]
    public static void RedirectToReferrer_MalformedReferer_RedirectsToPath()
    {
        var controller = CreateController("not-a-valid-uri", host: "example.com", path: "/current");

        RedirectUrl(controller.Referrer()).Should().Be("/current");
    }

    [Test]
    public static void RedirectToReferrer_NoReferer_RedirectsToPath()
    {
        var controller = CreateController(referer: null, host: "example.com", path: "/current");

        RedirectUrl(controller.Referrer()).Should().Be("/current");
    }

    [Test]
    public static void RedirectToReferrer_OffHostReferer_RedirectsToFallback()
    {
        var controller = CreateController("https://evil.example.net/x", host: "example.com", path: "/current");

        RedirectUrl(controller.Referrer("/fallback")).Should().Be("/fallback");
    }

    [Test]
    public static void RedirectToReferrer_SameHostReferer_RedirectsToReferer()
    {
        var controller = CreateController("https://example.com/back", host: "example.com", path: "/current");

        RedirectUrl(controller.Referrer()).Should().Be("https://example.com/back");
    }

    [Test]
    public static void RedirectToReturnUrl_LocalUrl_RedirectsToReturnUrl()
    {
        var controller = CreateController(referer: null, host: "example.com", path: "/current");
        controller.Url = LocalUrlHelper();

        RedirectUrl(controller.ReturnUrl("/safe", "/fallback")).Should().Be("/safe");
    }

    [Test]
    public static void RedirectToReturnUrl_NonLocalUrl_RedirectsToFallback()
    {
        var controller = CreateController(referer: null, host: "example.com", path: "/current");
        controller.Url = LocalUrlHelper();

        RedirectUrl(controller.ReturnUrl("https://evil.example.net", "/fallback")).Should().Be("/fallback");
    }

    private static TestController CreateController(string? referer, string host, string path)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Host = new HostString(host);
        httpContext.Request.Path = path;
        if (referer != null)
        {
            httpContext.Request.Headers.Referer = referer;
        }

        return new TestController
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private static IUrlHelper LocalUrlHelper()
    {
        var url = new Mock<IUrlHelper>();
        url.Setup(x => x.IsLocalUrl(It.IsAny<string?>()))
            .Returns<string?>(u => !string.IsNullOrEmpty(u) && u.StartsWith('/'));
        return url.Object;
    }

    private static string? RedirectUrl(IActionResult result) => ((RedirectResult)result).Url;

    private sealed class TestController : OdkControllerBase
    {
        public TestController()
            : base(Mock.Of<IRequestStore>(), Mock.Of<IOdkRoutes>())
        {
        }

        public IActionResult Referrer(string? fallback = null) => RedirectToReferrer(fallback);

        public IActionResult ReturnUrl(string? returnUrl, string fallback) => RedirectToReturnUrl(returnUrl, fallback);
    }
}
