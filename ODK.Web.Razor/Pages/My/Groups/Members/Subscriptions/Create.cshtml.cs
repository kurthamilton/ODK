using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.My.Groups.Members.Subscriptions;

public class CreateModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IPlatformProvider _platformProvider;

    public CreateModel(
        IChapterAdminService chapterAdminService,
        IPlatformProvider platformProvider)
    {
        _chapterAdminService = chapterAdminService;
        _platformProvider = platformProvider;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(SubscriptionFormSubmitViewModel viewModel)
    {
        var result = await _chapterAdminService.CreateChapterSubscription(AdminServiceRequest, new CreateChapterSubscription
        {
            Amount = viewModel.Amount ?? 0,
            ChapterId = ChapterId,
            Description = viewModel.Description,
            Disabled = !viewModel.Enabled,
            Months = viewModel.DurationMonths ?? 0,
            Name = viewModel.Name,
            Title = viewModel.Title,
            Type = viewModel.Type
        });

        AddFeedback(result, "Subscription created");

        if (!result.Success)
        {
            return Page();            
        }

        var platform = _platformProvider.GetPlatform();
        var chapter = await _chapterAdminService.GetChapter(AdminServiceRequest);
        return Redirect(OdkRoutes.MemberGroups.MembersSubscriptions(platform, chapter));
    }
}
