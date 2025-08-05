using Microsoft.AspNetCore.Mvc;
using ODK.Services.Events;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.SocialMedia;
using ODK.Services.Subscriptions;
using ODK.Web.Common.Config.Settings;

namespace ODK.Web.Razor.Controllers;

[Route("[controller]")]
[ApiController]
public class ScheduledTasksController : Controller
{
    private readonly IEventAdminService _eventAdminService;
    private readonly IInstagramService _instagramService;
    private readonly ILoggingService _loggingService;
    private readonly IMemberAdminService _memberAdminService;
    private readonly ScheduledTasksSettings _settings;
    private readonly ISiteSubscriptionService _siteSubscriptionService;

    public ScheduledTasksController(
        IEventAdminService eventAdminService,
        IInstagramService instagramService,
        AppSettings settings,
        ISiteSubscriptionService siteSubscriptionService,
        IMemberAdminService memberAdminService,
        ILoggingService loggingService)
    {
        _eventAdminService = eventAdminService;
        _instagramService = instagramService;
        _loggingService = loggingService;
        _memberAdminService = memberAdminService;
        _settings = settings.ScheduledTasks;
        _siteSubscriptionService = siteSubscriptionService;
    }

    [HttpPost("chapters/subscriptions/reminders")]
    public async Task SyncChapterSubscriptionReminders()
    {
        await _loggingService.Info("Scheduled tasks: sending chapter subscription reminders");

        AssertAuthorised();

        try
        {
            await _memberAdminService.SendMemberSubscriptionReminderEmails();
        }
        catch
        {
            // do nothing
        }
    }

    [HttpPost("emails")]
    public async Task SendScheduledEmails()
    {
        await _loggingService.Info("Scheduled tasks: sending scheduled emails");

        AssertAuthorised();

        try
        {
            await _eventAdminService.SendScheduledEmails();            
        }
        catch
        {
            // do nothing
        }        
    }

    [HttpPost("instagram")]
    public async Task ScrapeInstagramImages()
    {
        await _loggingService.Info("Scheduled tasks: scraping Instagram images");

        AssertAuthorised();

        try
        {
            await _instagramService.ScrapeLatestInstagramPosts();
        }
        catch
        {
            // do nothing
        }
    }

    [HttpPost("subscriptions/expired/sync")]
    public async Task SyncExpiredSubscriptions()
    {
        await _loggingService.Info("Scheduled tasks: syncing expired subscriptions");

        AssertAuthorised();

        try
        {
            await _siteSubscriptionService.SyncExpiredSubscriptions();
        }
        catch
        {
            // do nothing
        }
    }

    private void AssertAuthorised()
    {
        var header = Request.Headers.GetCommaSeparatedValues("X-API-KEY")
            .FirstOrDefault();
        
        if (header == _settings.ApiKey)
        {
            return;
        }

        throw new OdkNotAuthenticatedException();
    }
}
