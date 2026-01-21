using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.My.Groups.Members.Subscriptions;

public class CreateModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public CreateModel(
        IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(SubscriptionFormSubmitViewModel viewModel)
    {
        var result = await _chapterAdminService.CreateChapterSubscription(AdminServiceRequest, new CreateChapterSubscription
        {
            Amount = viewModel.Amount ?? 0,
            Description = viewModel.Description,
            Disabled = !viewModel.Enabled,
            Months = viewModel.DurationMonths ?? 0,
            Name = viewModel.Name,
            Recurring = viewModel.Recurring,
            SitePaymentSettingId = viewModel.SitePaymentSettingId,
            Title = viewModel.Title
        });

        AddFeedback(result, "Subscription created");

        if (!result.Success)
        {
            return Page();
        }

        var chapter = await GetChapter();
        return Redirect(OdkRoutes.MemberGroups.MembersSubscriptions(Platform, chapter));
    }
}