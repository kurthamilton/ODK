using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Services.Chapters.ViewModels;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class GroupModel : SiteAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public GroupModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public MemberChapterServiceRequest AdminServiceRequest
        => MemberChapterServiceRequest.Create(ChapterId, MemberServiceRequest);

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

        var request = CreateMemberChapterServiceRequest(id);

        var result = await _chapterAdminService.UpdateSiteAdminChapter(request, viewModel);

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return OnGet(id);
        }

        AddFeedback(new FeedbackViewModel("Chapter updated", FeedbackType.Success));
        return Redirect("/siteadmin/groups");
    }
}