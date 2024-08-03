using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class LinksModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public LinksModel(IRequestCache requestCache, IChapterAdminService chapterAdminService)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(ChapterLinksFormViewModel viewModel)
    {
        var serviceRequest = GetAdminServiceRequest();
        await _chapterAdminService.UpdateChapterLinks(serviceRequest, new UpdateChapterLinks
        {
            Facebook = viewModel.Facebook,
            Instagram = viewModel.Instagram,
            Twitter = viewModel.Twitter
        });

        AddFeedback(new FeedbackViewModel("Links updated", FeedbackType.Success));

        return RedirectToPage();
    }
}
