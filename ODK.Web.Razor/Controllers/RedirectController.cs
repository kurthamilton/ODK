using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers;

/// <summary>
/// A controller for routes that have been permanently moved
/// </summary>
[Route("")]
public class RedirectController : OdkControllerBase
{
    private readonly IRequestStore _requestStore;

    public RedirectController(IRequestStore requestStore)
        : base(requestStore)
    {
        _requestStore = requestStore;
    }

    [Route("{chapterName}/join")]
    public async Task<IActionResult> ChapterJoin(string chapterName)
    {
        var chapter = await _requestStore.GetChapter();
        var url = OdkRoutes.Account.Join(chapter);
        return RedirectPermanent(url);
    }

    [Route("{chapterName}/login")]
    public async Task<IActionResult> ChapterLogin(string chapterName)
    {
        var chapter = await _requestStore.GetChapter();
        var url = OdkRoutes.Account.Login(chapter);
        return RedirectPermanent(url);
    }
}