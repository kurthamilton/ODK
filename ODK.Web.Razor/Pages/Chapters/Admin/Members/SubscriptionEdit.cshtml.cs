using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class SubscriptionEditModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public SubscriptionEditModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public Guid SubscriptionId { get; set; }

    public IActionResult OnGet(Guid id)
    {
        SubscriptionId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, SubscriptionFormSubmitViewModel viewModel)
    {
        var serviceRequest = await CreateMemberChapterServiceRequest();
        var result = await _chapterAdminService.UpdateChapterSubscription(serviceRequest, id, new CreateChapterSubscription
        {
            Amount = viewModel.Amount ?? 0,
            Description = viewModel.Description,
            Disabled = !viewModel.Enabled,
            Name = viewModel.Name,
            Months = viewModel.DurationMonths ?? 0,
            Recurring = viewModel.Recurring,
            SitePaymentSettingId = null,
            Title = viewModel.Title
        });

        if (result.Success)
        {
            var chapter = await GetChapter();
            AddFeedback(new FeedbackViewModel("Subscription updated", FeedbackType.Success));
            return Redirect($"/{chapter.ShortName}/Admin/Members/Subscriptions");
        }

        AddFeedback(new FeedbackViewModel(result));
        return Page();
    }
}