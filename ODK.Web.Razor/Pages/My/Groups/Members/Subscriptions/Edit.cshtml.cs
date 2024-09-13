using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.My.Groups.Members.Subscriptions;

public class EditModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IPlatformProvider _platformProvider;

    public EditModel(IChapterAdminService chapterAdminService,
        IPlatformProvider platformProvider)
    {
        _chapterAdminService = chapterAdminService;
        _platformProvider = platformProvider;
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
            Name = viewModel.Name,
            Months = viewModel.DurationMonths ?? 0,
            Title = viewModel.Title,
            Type = viewModel.Type
        });

        AddFeedback(result, "Subscription updated");

        if (!result.Success)
        {
            return Page();            
        }

        var platform = _platformProvider.GetPlatform();
        var chapter = await _chapterAdminService.GetChapter(AdminServiceRequest);

        return Redirect(OdkRoutes.MemberGroups.MembersSubscriptions(platform, chapter));
    }
}
