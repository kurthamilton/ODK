using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Controllers;

[Route("[controller]")]
[ApiController]
public class InstagramController : Controller
{
    [AllowAnonymous]
    [HttpPost("Webhooks/Apify")]
    public async Task<IActionResult> ApifyWebhook()
    {
        using var sr = new StreamReader(Request.Body);
        var json = await sr.ReadToEndAsync();

        return new StatusCodeResult(200);
    }
}
