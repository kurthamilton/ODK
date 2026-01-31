using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Security;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.My.Groups.Members.Subscriptions;

public class EditModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public EditModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Subscriptions;

    public Guid SubscriptionId { get; private set; }

    public void OnGet(Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public async Task<IActionResult> OnPostAsync(Guid subscriptionId, SubscriptionFormSubmitViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest;
        var result = await _chapterAdminService.UpdateChapterSubscription(request, subscriptionId, new CreateChapterSubscription
        {
            Amount = viewModel.Amount ?? 0,
            Description = viewModel.Description,
            Disabled = !viewModel.Enabled,
            Name = viewModel.Name,
            Months = viewModel.DurationMonths ?? 0,
            Recurring = viewModel.Recurring,
            Title = viewModel.Title
        });

        AddFeedback(result, "Subscription updated");

        if (!result.Success)
        {
            return Page();
        }

        return Redirect(OdkRoutes.GroupAdmin.MembersSubscriptions(Chapter));
    }
}