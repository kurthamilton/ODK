using Microsoft.AspNetCore.Mvc;
using ODK.Core.SocialMedia;
using ODK.Services.SocialMedia;

namespace ODK.Web.Razor.Controllers
{
    [ApiController]
    public class ChapterController : Controller
    {
        private readonly IInstagramService _instagramService;

        public ChapterController(IInstagramService instagramService)
        {
            _instagramService = instagramService;
        }

        [HttpGet("{chapterName}/Instagram/Images/{id:guid}")]
        public async Task<IActionResult> GetInstagramImage(string chapterName, Guid id)
        {
            InstagramImage? image = await _instagramService.GetInstagramImage(id);
            if (image == null)
            {
                return NotFound();
            }

            return File(image.ImageData, image.MimeType);
        }
    }
}
