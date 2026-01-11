using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class QuestionEditModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public QuestionEditModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public ChapterQuestion Question { get; private set; } = null!;

    public async Task<IActionResult> OnGet(Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest();
        Question = await _chapterAdminService.GetChapterQuestion(serviceRequest, id);
        return Page();
    }
}