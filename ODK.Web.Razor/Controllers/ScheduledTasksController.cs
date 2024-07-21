using Microsoft.AspNetCore.Mvc;
using ODK.Services.Events;
using ODK.Services.SocialMedia;

namespace ODK.Web.Razor.Controllers;

[Route("[controller]")]
[ApiController]
public class ScheduledTasksController : Controller
{
    private readonly IInstagramService _instagramService;
    private readonly IEventAdminService _eventAdminService;

    public ScheduledTasksController(
        IEventAdminService eventAdminService,
        IInstagramService instagramService)
    {
        _eventAdminService = eventAdminService;
        _instagramService = instagramService;
    }

    [HttpPost("emails")]
    public async Task SendScheduledEmails()
    {
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
        try
        {
            await _instagramService.ScrapeLatestInstagramPosts();
        }
        catch
        {
            // do nothing
        }
    }
}
