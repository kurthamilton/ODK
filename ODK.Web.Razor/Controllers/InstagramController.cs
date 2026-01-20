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
    private readonly ISocialMediaService _socialMediaService;

    public InstagramController(
        ISocialMediaService socialMediaService,
        IImageService imageService,
        IRequestStore requestStore)
        : base(requestStore)
    {
        _imageService = imageService;
        _socialMediaService = socialMediaService;
    }

    [AllowAnonymous]
    [HttpGet("instagram/images/{id:guid}")]
    public async Task<IActionResult> GetInstagramImage(Guid id)
    {
        return await HandleVersionedRequest(
            version => _socialMediaService.GetInstagramImage(version, id),
            image => InstagramImageResult(image, null));
    }

    [AllowAnonymous]
    [HttpGet("instagram/images/{id:guid}/thumbnail")]
    public async Task<IActionResult> GetInstagramImageThumbnail(Guid id, int? size = null)
    {
        return await HandleVersionedRequest(
            version => _socialMediaService.GetInstagramImage(version, id),
            image => InstagramImageResult(image, size));
    }

    private IActionResult InstagramImageResult(InstagramImage? image, int? size = null)
    {
        if (image == null)
        {
            return NoContent();
        }

        var data = image.ImageData;

        if (size > 0)
        {
            data = _imageService.CropSquare(data);
            data = _imageService.Reduce(data, size.Value, size.Value);
        }

        return File(data, image.MimeType);
    }
}