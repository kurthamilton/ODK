using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.My.Groups.Members.Subscriptions;

public class EditModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;    

    public EditModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public Guid SubscriptionId { get; private set; }

    public void OnGet(Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public async Task<IActionResult> OnPostAsync(Guid subscriptionId, SubscriptionFormSubmitViewModel viewModel)
    {
        var result = await _chapterAdminService.UpdateChapterSubscription(AdminServiceRequest, subscriptionId, new CreateChapterSubscription
        {
            Amount = viewModel.Amount ?? 0,
            ChapterId = ChapterId,
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

        var chapter = await _chapterAdminService.GetChapter(AdminServiceRequest);

        return Redirect(OdkRoutes.MemberGroups.MembersSubscriptions(Platform, chapter));
    }
}
