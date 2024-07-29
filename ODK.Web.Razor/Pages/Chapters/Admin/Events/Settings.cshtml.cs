using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class SettingsModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public SettingsModel(IRequestCache requestCache, IChapterAdminService chapterAdminService)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(EventSettingsFormViewModel viewModel)
    {
        await _chapterAdminService.UpdateChapterEventSettings(CurrentMemberId, Chapter.Id, new UpdateChapterEventSettings
        {
            DefaultDayOfWeek = viewModel.DefaultDayOfWeek,
            DefaultDescription = viewModel.DefaultDescription,
            DefaultScheduledEmailDayOfWeek = viewModel.DefaultScheduledEmailDayOfWeek,
            DefaultScheduledEmailTimeOfDay = viewModel.DefaultScheduledEmailTimeOfDay,
            DisableComments = viewModel.DisableComments
        });

        AddFeedback(new FeedbackViewModel("Event settings updated", FeedbackType.Success));

        return RedirectToPage();
    }
}
