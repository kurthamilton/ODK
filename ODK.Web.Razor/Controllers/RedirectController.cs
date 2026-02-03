using Microsoft.AspNetCore.Mvc;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;

namespace ODK.Web.Razor.Controllers;

/// <summary>
/// A controller for routes that have been permanently moved
/// </summary>
[Route("")]
public class RedirectController : OdkControllerBase
{
    public RedirectController(
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
    }

    [Route("{chapterName}/join")]
    public async Task<IActionResult> ChapterJoin(string chapterName)
    {
        var url = OdkRoutes.Account.Join(Chapter);
        return RedirectPermanent(url);
    }

    [Route("{chapterName}/login")]
    public async Task<IActionResult> ChapterLogin(string chapterName)
    {
        var url = OdkRoutes.Account.Login(Chapter);
        return RedirectPermanent(url);
    }
}