using Microsoft.AspNetCore.Mvc;
using ODK.Services.Exceptions;
using ODK.Services.Geolocation;
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
    private readonly IIpLocationDatabaseService _ipLocationDatabaseService;
    private readonly ILoggingService _loggingService;
    private readonly IMemberAdminService _memberAdminService;
    private readonly IMemberInviteService _memberInviteService;
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
        IOdkRoutes odkRoutes,
        IMemberInviteService memberInviteService,
        IIpLocationDatabaseService ipLocationDatabaseService)
        : base(requestStore, odkRoutes)
    {
        _ipLocationDatabaseService = ipLocationDatabaseService;
        _loggingService = loggingService;
        _memberAdminService = memberAdminService;
        _memberInviteService = memberInviteService;
        _settings = settings;
        _siteSubscriptionService = siteSubscriptionService;
        _socialMediaService = socialMediaService;
    }

    /// <summary>
    /// Reminds members whose group membership is expiring. Scoped to the chapters this platform owns, so it
    /// belongs on the cron of *every* deployment - unlike the Instagram scrape below, which is
    /// platform-agnostic and so belongs on exactly one.
    /// </summary>
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

    [HttpPost("members/invites/purge")]
    public async Task PurgeExpiredInvitates()
    {
        AssertAuthorised();

        try
        {
            await _memberInviteService.PurgeExpiredInvites();
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

    /// <summary>
    /// Scrapes every group's Instagram account, whichever platform owns the group - see
    /// <c>SocialMediaService.ScrapeLatestInstagramPosts</c>. Belongs on exactly one deployment's cron:
    /// scheduling it on both scrapes every group twice.
    /// </summary>
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

    [HttpPost("geoip/update")]
    public async Task UpdateIpLocationDatabase()
    {
        AssertAuthorised();

        try
        {
            await _ipLocationDatabaseService.Update();
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