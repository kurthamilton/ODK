using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class SubscriptionEditModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public SubscriptionEditModel(IRequestCache requestCache, IChapterAdminService chapterAdminService) 
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public ChapterSubscription Subscription { get; private set; } = null!;

    public async Task<IActionResult> OnGet(Guid id)
    {
        ChapterSubscription? subscription = await _chapterAdminService.GetChapterSubscription(CurrentMemberId, id);
        if (subscription == null)
        {
            return NotFound();
        }

        Subscription = subscription;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, SubscriptionFormViewModel viewModel)
    {
        ServiceResult result = await _chapterAdminService.UpdateChapterSubscription(CurrentMemberId, id, new CreateChapterSubscription
        {
            Amount = viewModel.Amount ?? 0,
            ChapterId = Chapter.Id,
            Description = viewModel.Description,
            Name = viewModel.Name,
            Months = viewModel.DurationMonths ?? 0,
            Title = viewModel.Title,
            Type = viewModel.Type

        });

        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Subscription updated", FeedbackType.Success));
            return Redirect($"/{Chapter.Name}/Admin/Members/Subscriptions");
        }

        AddFeedback(new FeedbackViewModel(result));
        return Page();
    }
}
