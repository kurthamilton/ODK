using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.SocialMedia;
using ODK.Services.Imaging;
using ODK.Services.SocialMedia;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers;

[ApiController]
public class InstagramController : OdkControllerBase
{
    private readonly IImageService _imageService;
    private readonly IInstagramService _instagramService;

    public InstagramController(
        IInstagramService instagramService,
        IImageService imageService,
        IRequestStore requestStore)
        : base(requestStore)
    {
        _imageService = imageService;
        _instagramService = instagramService;
    }

    [AllowAnonymous]
    [HttpGet("{chapterName}/Instagram/Images/{id:guid}")]
    public async Task<IActionResult> GetInstagramImage(string chapterName, Guid id)
    {
        return await HandleVersionedRequest(
            version => _instagramService.GetInstagramImage(version, id),
            InstagramImageResult);
    }

    private IActionResult InstagramImageResult(InstagramImage? image)
    {
        if (image == null)
        {
            return NoContent();
        }

        var data = _imageService.Reduce(image.ImageData, 150, 150);
        return File(data, image.MimeType);
    }
}
