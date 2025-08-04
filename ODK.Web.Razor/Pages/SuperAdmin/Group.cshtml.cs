using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Settings;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class GroupModel : SuperAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public GroupModel(
        IRequestCache requestCache,
        IChapterAdminService chapterAdminService) 
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public Guid ChapterId { get; private set; }

    public IActionResult OnGet(Guid id)
    {
        ChapterId = id;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, SuperAdminChapterUpdateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return OnGet(id);
        }

        var request = new AdminServiceRequest(id, CurrentMemberId);

        var result = await _chapterAdminService.UpdateSuperAdminChapter(request, viewModel);

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return OnGet(id);
        }

        AddFeedback(new FeedbackViewModel("Chapter updated", FeedbackType.Success));
        return Redirect("/superadmin/groups");
    }
}
