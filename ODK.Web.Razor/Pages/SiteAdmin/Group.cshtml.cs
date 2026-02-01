using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.ViewModels;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class GroupModel : SiteAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IChapterSiteAdminService _chapterSiteAdminService;

    public GroupModel(
        IChapterAdminService chapterAdminService, 
        IChapterSiteAdminService chapterSiteAdminService)
    {
        _chapterAdminService = chapterAdminService;
        _chapterSiteAdminService = chapterSiteAdminService;
    }

    public Guid ChapterId { get; private set; }

    public IActionResult OnGet(Guid id)
    {
        ChapterId = id;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, SiteAdminChapterUpdateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return OnGet(id);
        }

        var request = MemberChapterServiceRequest;

        var result = await _chapterSiteAdminService.UpdateSiteAdminChapter(request, id, viewModel);

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return OnGet(id);
        }

        AddFeedback(new FeedbackViewModel("Group updated", FeedbackType.Success));
        return Redirect(OdkRoutes.SiteAdmin.Groups);
    }
}