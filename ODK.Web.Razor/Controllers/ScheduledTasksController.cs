using Microsoft.AspNetCore.Mvc;
using ODK.Services.Events;

namespace ODK.Web.Razor.Controllers;

[Route("[controller]")]
[ApiController]
public class ScheduledTasksController : Controller
{
    private readonly IEventAdminService _eventAdminService;

    public ScheduledTasksController(IEventAdminService eventAdminService)
    {
        _eventAdminService = eventAdminService;
    }

    [HttpPost("run")]
    public async Task Run()
    {
        await _eventAdminService.SendScheduledEmails();
    }
}
