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
    /// Moves members whose site subscription has expired past the cooldown onto the platform's default
    /// plan. Scoped to the plans this platform sells, so it belongs on the cron of *every* deployment -
    /// and the member is told about it as the platform whose plan lapsed.
    /// </summary>
    [HttpPost("subscriptions/lapsed/downgrade")]
    public async Task DowngradeLapsedSiteSubscriptions()
    {
        AssertAuthorised();

        await Run(
            nameof(DowngradeLapsedSiteSubscriptions),
            () => _siteSubscriptionService.DowngradeLapsedSubscriptions(ServiceRequest));
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

        await Run(
            nameof(SyncChapterSubscriptionReminders),
            () => _memberAdminService.SendMemberSubscriptionReminderEmails(ServiceRequest));
    }

    [HttpPost("members/invites/purge")]
    public async Task PurgeExpiredInvitates()
    {
        AssertAuthorised();

        await Run(
            nameof(PurgeExpiredInvitates),
            () => _memberInviteService.PurgeExpiredInvites());
    }

    [HttpPost("logs/purge")]
    public async Task PurgeLogs()
    {
        AssertAuthorised();

        await Run(nameof(PurgeLogs), () => _loggingService.PurgeLogs());
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

        await Run(
            nameof(ScrapeInstagramImages),
            () => _socialMediaService.ScrapeLatestInstagramPosts());
    }

    [HttpPost("geoip/update")]
    public async Task UpdateIpLocationDatabase()
    {
        AssertAuthorised();

        await Run(
            nameof(UpdateIpLocationDatabase),
            () => _ipLocationDatabaseService.Update());
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

    /// <summary>
    /// Runs a task, reporting whatever it threw rather than rethrowing it. A cron has nothing to do with a
    /// failure and no eye on the response, so every endpoint answers the same way whatever happened - which
    /// is exactly why the failure has to be recorded here, or a task that has been failing for weeks looks
    /// from the outside like one that runs cleanly.
    /// <para>
    /// Logging is itself guarded, because whatever took the task down - the database, most of the time - is
    /// the same thing the log is written to, and an endpoint that answers 500 only when logging also fails
    /// would report the least useful cases and stay silent about the rest.
    /// </para>
    /// </summary>
    private async Task Run(string task, Func<Task> run)
    {
        try
        {
            await run();
        }
        catch (Exception exception)
        {
            try
            {
                await _loggingService.Error($"Error running scheduled task '{task}'", exception);
            }
            catch
            {
                // do nothing - see above
            }
        }
    }
}