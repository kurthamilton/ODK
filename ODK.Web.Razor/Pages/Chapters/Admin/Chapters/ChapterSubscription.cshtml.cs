using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class ChapterSubscriptionModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public ChapterSubscriptionModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.SiteSubscription;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var result = await _chapterAdminService.CancelSiteSubscription(MemberChapterAdminServiceRequest, id);        
        AddFeedback(result, "Subscription cancelled");
        return RedirectToPage();
    }
}