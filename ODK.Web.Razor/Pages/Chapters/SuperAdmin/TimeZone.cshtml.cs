using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin
{
    public class TimeZoneModel : SuperAdminPageModel
    {
        private readonly IChapterAdminService _chapterAdminService;

        public TimeZoneModel(IRequestCache requestCache, IChapterAdminService chapterAdminService)
            : base(requestCache)
        {
            _chapterAdminService = chapterAdminService;
        }

        public async Task<IActionResult> OnPostAsync(TimeZoneFormViewModel viewModel)
        {
            var result = await _chapterAdminService.UpdateChapterTimeZone(CurrentMemberId, Chapter, viewModel.TimeZone);
            if (result.Success)
            {
                AddFeedback(new FeedbackViewModel("Time zone updated", FeedbackType.Success));
                return RedirectToPage();
            }

            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }
    }
}
