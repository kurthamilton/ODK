using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
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

            if (viewModel.Location.Lat == null || viewModel.Location.Long == null)
            {
                AddFeedback("Location not set", FeedbackType.Error);
                return Page();
            }

            var result = await _chapterService.CreateChapter(CurrentMemberId, new ChapterCreateModel
            {
                Description = viewModel.Description ?? "",
                Location = new LatLong(viewModel.Location.Lat.Value, viewModel.Location.Long.Value),
                LocationName = viewModel.Location.Name,
                Name = viewModel.Name ?? "",
                TimeZoneId = viewModel.TimeZoneId
            });

            if (!result.Success)
            {
                AddFeedback(result);
                return Page();
            }

            AddFeedback("Group created. Once approved you will be able to publish and start accepting group members.", FeedbackType.Success);

            return result.Value != null
                ? Redirect(OdkRoutes2.MemberGroups.Group(result.Value.Id))
                : Redirect(OdkRoutes2.MemberGroups.Index());
        }
    }
}
