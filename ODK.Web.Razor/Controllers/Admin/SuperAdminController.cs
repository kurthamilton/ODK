using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Emails;
using ODK.Services.Logging;
using ODK.Services.Settings;
using ODK.Services.SocialMedia;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Controllers.Admin;

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

    [HttpGet("{chapterName}/Admin/SuperAdmin")]
    public IActionResult Index(string chapterName)
    {
        return Redirect($"/{chapterName}/Admin/SuperAdmin/Payments");
    }

    [HttpPost("{chapterName}/Admin/SuperAdmin/EmailProviders/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteEmailProvider(Guid id)
    {
        await _settingsService.DeleteEmailProvider(MemberId, id);

        AddFeedback(new FeedbackViewModel("Email provider deleted", FeedbackType.Success));

        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/SuperAdmin/Errors/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteError(string chapterName, Guid id)
    {
        await _loggingService.DeleteError(MemberId, id);

        return Redirect($"/{chapterName}/Admin/SuperAdmin/Errors");
    }

    [HttpPost("{chapterName}/Admin/SuperAdmin/Errors/{id:Guid}/DeleteAll")]
    public async Task<IActionResult> DeleteAllErrors(string chapterName, Guid id)
    {
        await _loggingService.DeleteAllErrors(MemberId, id);

        return Redirect($"/{chapterName}/Admin/SuperAdmin/Errors");
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
