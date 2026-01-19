using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;

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
        var serviceRequest = await CreateMemberChapterServiceRequest();
        await _chapterAdminService.UpdateChapterRedirectUrl(serviceRequest, redirectUrl);
        AddFeedback("Redirect updated", FeedbackType.Success);
        return RedirectToPage();
    }
}