using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Security;
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

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Subscriptions;

    public Guid SubscriptionId { get; set; }

    public IActionResult OnGet(Guid id)
    {
        SubscriptionId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, SubscriptionFormSubmitViewModel viewModel)
    {
        var serviceRequest = MemberChapterAdminServiceRequest;
        var result = await _chapterAdminService.UpdateChapterSubscription(serviceRequest, id, new CreateChapterSubscription
        {
            Amount = viewModel.Amount ?? 0,
            Description = viewModel.Description,
            Disabled = !viewModel.Enabled,
            Name = viewModel.Name,
            Months = viewModel.DurationMonths ?? 0,
            Recurring = viewModel.Recurring,
            Title = viewModel.Title
        });

        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Subscription updated", FeedbackType.Success));
            return Redirect(OdkRoutes.GroupAdmin.MembersSubscriptions(Chapter));
        }

        AddFeedback(new FeedbackViewModel(result));
        return Page();
    }
}