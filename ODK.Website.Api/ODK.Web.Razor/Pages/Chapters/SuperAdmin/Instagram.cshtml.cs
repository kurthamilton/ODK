using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Settings;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin
{
    public class InstagramModel : SuperAdminPageModel
    {
        private readonly ISettingsService _settingsService;

        public InstagramModel(IRequestCache requestCache, ISettingsService settingsService) 
            : base(requestCache)
        {
            _settingsService = settingsService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(InstagramFormViewModel viewModel)
        {
            await _settingsService.UpdateInstagramSettings(Chapter.Id, CurrentMemberId, viewModel.Scrape, viewModel.ScraperUserAgent);

            AddFeedback(new FeedbackViewModel("Instagram settings updated", FeedbackType.Success));

            return RedirectToPage();
        }
    }
}
