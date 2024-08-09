using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.SocialMedia;

namespace ODK.Web.Razor.Controllers;

[ApiController]
public class InstagramController : Controller
{
    private readonly IInstagramService _instagramService;

    public InstagramController(IInstagramService instagramService)
    {
        _instagramService = instagramService;
    }

    [AllowAnonymous]
    [HttpGet("{chapterName}/Instagram/Images/{id:guid}")]
    public async Task<IActionResult> GetInstagramImage(string chapterName, Guid id)
    {
        var image = await _instagramService.GetInstagramImage(id);
        return File(image.ImageData, image.MimeType);
    }
}
