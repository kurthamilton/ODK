using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Security;
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

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Subscriptions;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(SubscriptionFormSubmitViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest;
        var result = await _chapterAdminService.CreateChapterSubscription(request, new ChapterSubscriptionCreateModel
        {
            Amount = viewModel.Amount ?? 0,
            Description = viewModel.Description,
            Disabled = !viewModel.Enabled,
            Months = viewModel.DurationMonths ?? 0,
            Name = viewModel.Name,
            Recurring = viewModel.Recurring,
            Title = viewModel.Title
        });

        AddFeedback(result, "Subscription created");

        if (!result.Success)
        {
            return Page();
        }

        var path = OdkRoutes.GroupAdmin.Subscriptions(Chapter).Path;
        return Redirect(path);
    }
}