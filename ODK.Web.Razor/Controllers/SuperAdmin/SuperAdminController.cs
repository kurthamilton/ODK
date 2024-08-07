using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Emails;
using ODK.Services.Logging;
using ODK.Services.Settings;
using ODK.Services.SocialMedia;

namespace ODK.Web.Razor.Controllers.SuperAdmin;

public class SuperAdminController : OdkControllerBase
{
    private readonly IEmailAdminService _emailAdminService;
    private readonly IInstagramService _instagramService;
    private readonly ILoggingService _loggingService;
    private readonly IRequestCache _requestCache;
    private readonly ISettingsService _settingsService;

    public SuperAdminController(IEmailAdminService emailAdminService,
        ILoggingService loggingService, IInstagramService instagramService,
        IRequestCache requestCache, ISettingsService settingsService)
    {
        _emailAdminService = emailAdminService;
        _instagramService = instagramService;
        _loggingService = loggingService;
        _requestCache = requestCache;
        _settingsService = settingsService;
    }

    [HttpGet("SuperAdmin")]
    public IActionResult Index()
    {
        return Redirect("/SuperAdmin/Emails");
    }

    [HttpGet("{chapterName}/Admin/SuperAdmin")]
    public IActionResult Index(string chapterName)
    {
        return Redirect($"/{chapterName}/Admin/SuperAdmin/Payments");
    }    

    [HttpPost("SuperAdmin/Errors/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteError(Guid id)
    {
        await _loggingService.DeleteError(MemberId, id);

        return Redirect("/SuperAdmin/Errors");
    }

    [HttpPost("SuperAdmin/Errors/{id:Guid}/DeleteAll")]
    public async Task<IActionResult> DeleteAllErrors(Guid id)
    {
        await _loggingService.DeleteAllErrors(MemberId, id);

        return Redirect("/SuperAdmin/Errors");
    }

    [HttpPost("{chapterName}/Admin/SuperAdmin/Instagram/Scrape")]
    public async Task<IActionResult> ScrapeInstagram(string chapterName)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        if (chapter == null)
        {
            return RedirectToReferrer();
        }

        await _instagramService.ScrapeLatestInstagramPosts(chapter.Id);

        return RedirectToReferrer();
    }
}
