using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SuperAdmin;

public class RedirectModel : ChapterSuperAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public RedirectModel(IRequestCache requestCache, IChapterAdminService chapterAdminService)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? redirectUrl)
    {
        var serviceRequest = await GetAdminServiceRequest();
        await _chapterAdminService.UpdateChapterRedirectUrl(serviceRequest, redirectUrl);
        AddFeedback("Redirect updated", FeedbackType.Success);
        return RedirectToPage();
    }
}