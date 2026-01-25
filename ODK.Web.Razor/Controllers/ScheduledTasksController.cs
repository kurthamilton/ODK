using Microsoft.AspNetCore.Mvc;
using ODK.Services.Exceptions;
using ODK.Services.Members;
using ODK.Services.SocialMedia;
using ODK.Services.Subscriptions;
using ODK.Web.Common.Config.Settings;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers;

[Route("[controller]")]
[ApiController]
public class ScheduledTasksController : OdkControllerBase
{
    private readonly IMemberAdminService _memberAdminService;
    private readonly ScheduledTasksSettings _settings;
    private readonly ISiteSubscriptionService _siteSubscriptionService;
    private readonly ISocialMediaService _socialMediaService;

    public ScheduledTasksController(
        ISocialMediaService socialMediaService,
        AppSettings settings,
        ISiteSubscriptionService siteSubscriptionService,
        IMemberAdminService memberAdminService,
        IRequestStore requestStore)
        : base(requestStore)
    {
        _memberAdminService = memberAdminService;
        _settings = settings.ScheduledTasks;
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

    [HttpPost("subscriptions/expired/sync")]
    public async Task SyncExpiredSubscriptions()
    {
        AssertAuthorised();

        try
        {
            await _siteSubscriptionService.SyncExpiredSubscriptions(ServiceRequest);
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