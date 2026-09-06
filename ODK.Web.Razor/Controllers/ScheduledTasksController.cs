using Microsoft.AspNetCore.Mvc;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.SocialMedia;
using ODK.Services.Subscriptions;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Common.Settings;

namespace ODK.Web.Razor.Controllers;

[Route("[controller]")]
[ApiController]
[IgnoreAntiforgeryToken] // external cron POSTs; authenticated by the ScheduledTasks API key, not a token
public class ScheduledTasksController : OdkControllerBase
{
    private readonly ILoggingService _loggingService;
    private readonly IMemberAdminService _memberAdminService;
    private readonly ScheduledTasksControllerSettings _settings;
    private readonly ISiteSubscriptionService _siteSubscriptionService;
    private readonly ISocialMediaService _socialMediaService;

    public ScheduledTasksController(
        ISocialMediaService socialMediaService,
        ScheduledTasksControllerSettings settings,
        ISiteSubscriptionService siteSubscriptionService,
        IMemberAdminService memberAdminService,
        ILoggingService loggingService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _loggingService = loggingService;
        _memberAdminService = memberAdminService;
        _settings = settings;
        _siteSubscriptionService = siteSubscriptionService;
        _socialMediaService = socialMediaService;
    }

    [HttpPost("chapters/subscriptions/reminders")]
    public async Task SyncChapterSubscriptionReminders()
    {
        AssertAuthorised();

        try
        {
            await _memberAdminService.SendMemberSubscriptionReminderEmails(ServiceRequest);
        }
        catch
        {
            // do nothing
        }
    }

    [HttpPost("logs/purge")]
    public async Task PurgeLogs()
    {
        AssertAuthorised();

        try
        {
            await _loggingService.PurgeLogs();
        }
        catch
        {
            // do nothing
        }
    }

    [HttpPost("instagram")]
    public async Task ScrapeInstagramImages()
    {
        AssertAuthorised();

        try
        {
            await _socialMediaService.ScrapeLatestInstagramPosts();
        }
        catch
        {
            // do nothing
        }
    }

    private void AssertAuthorised()
    {
        var header = Request.Headers.GetCommaSeparatedValues(_settings.ApiKeyHeader)
            .FirstOrDefault();

        if (header == _settings.ApiKey)
        {
            return;
        }

        throw new OdkNotAuthenticatedException();
    }
}