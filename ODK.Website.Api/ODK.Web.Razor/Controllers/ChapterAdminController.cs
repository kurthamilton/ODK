using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ChapterAdminController : OdkControllerBase
    {
        private readonly IChapterAdminService _chapterAdminService;

        public ChapterAdminController(IChapterAdminService chapterAdminService)
        {
            _chapterAdminService = chapterAdminService;
        }

        [HttpPost("/{chapterName}/Admin/Chapter/Text")]
        public async Task<IActionResult> UpdateChapterTexts(string chapterName, [FromForm] ChapterTextsFormViewModel viewModel)
        {
            ServiceResult result = await _chapterAdminService.UpdateChapterTexts(MemberId, chapterName, new UpdateChapterTexts
            {
                RegisterText = viewModel.RegisterMessage,
                WelcomeText = viewModel.WelcomeMessage
            });

            if (!result.Success)
            {
                AddFeedback(new FeedbackViewModel(result));
            }

            return RedirectToReferrer();
        }
    }
}
