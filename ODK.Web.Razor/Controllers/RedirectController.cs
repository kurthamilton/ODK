using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Controllers;

/// <summary>
/// A controller for routes that have been permanently moved
/// </summary>
[Route("")]
public class RedirectController : Controller
{
    private readonly IRequestCache _requestCache;

    public RedirectController(IRequestCache requestCache)
    {
        _requestCache = requestCache;
    }

    [Route("{chapterName}/join")]
    public async Task<IActionResult> ChapterJoin(string chapterName)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var url = OdkRoutes.Account.Join(chapter);
        return RedirectPermanent(url);
    }

    [Route("{chapterName}/login")]
    public async Task<IActionResult> ChapterLogin(string chapterName)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var url = OdkRoutes.Account.Login(chapter);
        return RedirectPermanent(url);
    }
}
