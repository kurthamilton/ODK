using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class SubscriptionCreateModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public SubscriptionCreateModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(SubscriptionFormSubmitViewModel viewModel)
    {
        var serviceRequest = await CreateMemberChapterServiceRequest();
        var result = await _chapterAdminService.CreateChapterSubscription(serviceRequest, new CreateChapterSubscription
        {
            Amount = viewModel.Amount ?? 0,
            Description = viewModel.Description,
            Disabled = !viewModel.Enabled,
            Months = viewModel.DurationMonths ?? 0,
            Name = viewModel.Name,
            Recurring = viewModel.Recurring,
            Title = viewModel.Title
        });

        if (result.Success)
        {
            var chapter = await GetChapter();
            AddFeedback(new FeedbackViewModel("Subscription created", FeedbackType.Success));
            return Redirect($"/{chapter.ShortName}/Admin/Members/Subscriptions");
        }

        AddFeedback(new FeedbackViewModel(result));
        return Page();
    }
}