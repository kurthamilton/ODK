using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.ViewModels;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class GroupModel : SiteAdminPageModel
{
    private readonly IChapterSiteAdminService _chapterSiteAdminService;

    public GroupModel(IChapterSiteAdminService chapterSiteAdminService)
    {
        _chapterSiteAdminService = chapterSiteAdminService;
    }

    public async Task<IActionResult> OnPostAsync(Guid id, SiteAdminChapterUpdateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = MemberChapterServiceRequest;

        var result = await _chapterSiteAdminService.UpdateSiteAdminChapter(request, viewModel);

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Group updated", FeedbackType.Success);
        return Redirect(OdkRoutes.SiteAdmin.Groups);
    }
}