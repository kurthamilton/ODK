using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Chapters;

namespace ODK.Web.Razor.Pages.Groups.Admin
{
    public class CreateModel : OdkPageModel
    {
        private readonly IChapterService _chapterService;

        public CreateModel(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync([FromForm] CreateChapterSubmitViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _chapterService.CreateChapter(CurrentMemberId, new ChapterCreateModel
            {
                CountryId = viewModel.CountryId,
                Description = viewModel.Description ?? "",
                Name = viewModel.Name ?? "",
                TimeZoneId = viewModel.TimeZoneId
            });

            if (!result.Success)
            {
                AddFeedback(result);
                return Page();
            }

            AddFeedback("Group created", FeedbackType.Success);

            return result.Value != null
                ? Redirect(OdkRoutes2.MemberGroups.Group(result.Value.Id))
                : Redirect(OdkRoutes2.MemberGroups.Index());
        }
    }
}
