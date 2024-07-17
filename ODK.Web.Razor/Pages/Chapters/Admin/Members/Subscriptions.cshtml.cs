using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class SubscriptionsModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public SubscriptionsModel(IRequestCache requestCache, IChapterAdminService chapterAdminService) 
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(MembershipSettingsFormViewModel viewModel)
    {
        var result = await _chapterAdminService.UpdateChapterMembershipSettings(CurrentMemberId, Chapter.Id, 
            new UpdateChapterMembershipSettings
            {
                Enabled = viewModel.Enabled,
                MembershipDisabledAfterDaysExpired = viewModel.MembershipDisabledAfter,
                TrialPeriodMonths = viewModel.TrialPeriodMonths
            });

        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Membership settings updated", FeedbackType.Success));
            return RedirectToPage();
        }

        AddFeedback(new FeedbackViewModel(result));
        return Page();
    }
}
