using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SiteAdmin;

public class RedirectModel : ChapterSiteAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public RedirectModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? redirectUrl)
    {
        var request = MemberChapterAdminServiceRequest;
        await _chapterAdminService.UpdateChapterRedirectUrl(request, redirectUrl);
        AddFeedback("Redirect updated", FeedbackType.Success);
        return RedirectToPage();
    }
}