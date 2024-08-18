using Microsoft.AspNetCore.Mvc;
using ODK.Services.Events;
using ODK.Services.Exceptions;
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
    private readonly ScheduledTasksSettings _settings;
    private readonly ISiteSubscriptionService _siteSubscriptionService;

    public ScheduledTasksController(
        IEventAdminService eventAdminService,
        IInstagramService instagramService,
        AppSettings settings,
        ISiteSubscriptionService siteSubscriptionService)
    {
        _eventAdminService = eventAdminService;
        _instagramService = instagramService;
        _settings = settings.ScheduledTasks;
        _siteSubscriptionService = siteSubscriptionService;
    }

    [HttpPost("emails")]
    public async Task SendScheduledEmails()
    {
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
