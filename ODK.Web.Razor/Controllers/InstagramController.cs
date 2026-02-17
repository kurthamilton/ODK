using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Imaging;
using ODK.Services.SocialMedia;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Attributes;

namespace ODK.Web.Razor.Controllers;

[ApiController]
public class InstagramController : OdkControllerBase
{
    private readonly IImageService _imageService;
    private readonly ISocialMediaService _socialMediaService;

    public InstagramController(
        ISocialMediaService socialMediaService,
        IImageService imageService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _imageService = imageService;
        _socialMediaService = socialMediaService;
    }

    [SkipRequestStoreMiddleware]
    [AllowAnonymous]
    [HttpGet("instagram/images/{id:guid}")]
    public async Task<IActionResult> GetInstagramImage(Guid id, [FromQuery(Name = "v")] int? version = null)
    {
        var image = await _socialMediaService.GetInstagramImage(id);
        if (image == null)
        {
            return NotFound();
        }

        return CacheableFile(image.ImageData, image.MimeType, version);
    }

    [SkipRequestStoreMiddleware]
    [AllowAnonymous]
    [HttpGet("instagram/images/{id:guid}/thumbnail")]
    public async Task<IActionResult> GetInstagramImageThumbnail(Guid id, int? size = null, [FromQuery(Name = "v")] int? version = null)
    {
        var image = await _socialMediaService.GetInstagramImage(id);
        if (image == null)
        {
            return NotFound();
        }

        var data = image.ImageData;

        if (size > 0)
        {
            data = _imageService.CropSquare(data);
            data = _imageService.Reduce(data, size.Value, size.Value);
        }

        return CacheableFile(data, image.MimeType, version);
    }
}